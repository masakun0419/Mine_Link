# 05 通信・API設計

## 1. 通信分類

| 通信 | プロトコル | 用途 |
|---|---|---|
| REST API | HTTPS | ルーム作成、参加、更新確認 |
| 制御チャネル | WebSocket Secure | 状態通知、参加・退出、ハートビート |
| データチャネル | TCP + TLS | Minecraft Java版の双方向転送 |

初版では実装を単純化するため、制御とMinecraftデータを論理的に分離する。将来はQUIC等への変更を検討できるよう、`ITunnelTransport`で抽象化する。

## 2. REST API案

### 2.1 ルーム作成

`POST /api/v1/rooms`

```json
{
  "displayName": "Masakun",
  "edition": "java",
  "maxGuests": 5,
  "approvalMode": "automatic"
}
```

```json
{
  "roomId": "01992db0-0000-7000-8000-000000000001",
  "joinCode": "MK7P-4Q2N",
  "hostToken": "<opaque-token>",
  "expiresAt": "2026-08-25T10:00:00Z",
  "relay": {
    "region": "jp-east",
    "host": "relay.example.net",
    "port": 443
  }
}
```

### 2.2 ルーム参加

`POST /api/v1/rooms/join`

```json
{
  "joinCode": "MK7P-4Q2N",
  "displayName": "Guest01",
  "clientVersion": "0.1.0"
}
```

### 2.3 ルーム終了

`DELETE /api/v1/rooms/{roomId}`

ホストトークンをAuthorizationヘッダーで送信する。

## 3. 制御メッセージ

```json
{
  "type": "guest.connected",
  "messageId": "01992db0-0000-7000-8000-000000000002",
  "timestamp": "2026-08-25T09:10:00Z",
  "payload": {
    "guestId": "guest_01",
    "displayName": "Guest01"
  }
}
```

主な`type`は以下とする。

- `hello`
- `heartbeat.ping` / `heartbeat.pong`
- `guest.join.requested`
- `guest.join.approved`
- `guest.connected`
- `guest.disconnected`
- `tunnel.open`
- `tunnel.close`
- `room.closed`
- `error`

## 4. データチャネル

MinecraftのTCP接続1本につき、1つの`sessionId`を割り当てる。

```text
Magic       4 byte   "MLNK"
Version     1 byte
Flags       1 byte
HeaderLen   2 byte
SessionId  16 byte
PayloadLen  4 byte
Payload     n byte
```

初期実装では、TLSストリーム上でヘッダーとペイロードを送る。上限サイズを設け、異常な長さは接続を終了する。

## 5. エラーレスポンス

```json
{
  "error": {
    "code": "ROOM_CODE_EXPIRED",
    "message": "The join code has expired.",
    "traceId": "00-abcd1234"
  }
}
```

HTTPステータスは意味に合わせ、認証失敗をすべて`500`へ丸めない。

| 状況 | Status |
|---|---:|
| 入力不正 | 400 |
| 認証失敗 | 401 |
| 権限不足 | 403 |
| ルームなし | 404 |
| 満員・競合 | 409 |
| レート制限 | 429 |
| サーバー障害 | 500 / 503 |

## 6. タイムアウト・再接続

| 項目 | 初期値 |
|---|---:|
| API接続タイムアウト | 10秒 |
| Relay接続タイムアウト | 15秒 |
| ハートビート間隔 | 15秒 |
| 切断判定 | 45秒 |
| 自動再接続 | 1秒、3秒、10秒 |
| Minecraft接続アイドル上限 | 30分（設定可能） |

