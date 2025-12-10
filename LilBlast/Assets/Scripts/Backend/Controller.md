# Lil Games — API Controllers and Endpoints (Master Plan)

This document defines all backend controllers, endpoints, and their request/response DTO structures.

---

# 1. AuthController

## POST /auth/sign-in

Request:

* email
* username
* password

Response:

* userId
* authToken

## POST /auth/login

Request:

* username
* password

Response:

* userId
* authToken

## POST /auth/guest

Request:

* deviceId

Response:

* userId
* authToken

## POST /auth/oauth

Request:

* provider (GOOGLE, FACEBOOK)
* oauthToken

Response:

* userId
* authToken

## POST /auth/upgrade-guest

Request:

* userId
* email
* username
* password

Response:

* success

---

# 2. UserController

## GET /users/{userId}

Response:

* userId
* username
* avatarUrl
* createdAt

## PUT /users/{userId}/avatar

Request:

* avatarUrl

Response:

* success

---

# 3. ProfileController

## GET /profile/{userId}

Response:

* totalAttempts
* totalWins
* threeStarCount
* totalPowerupsUsed
* mostUsedPowerup
* totalScore
* lastLevelReached

---

# 4. InventoryController

## GET /inventory/{userId}

Response:

* coins
* lives
* shuffleCount
* powerShuffleCount
* manipulateCount
* destroyCount

## POST /inventory/consume

Request:

* userId
* itemType (COINS, LIVES, SHUFFLE, POWERSHUFFLE, MANIPULATE, DESTROY)
* amount

Response:

* success

## POST /inventory/add

Request:

* userId
* itemType
* amount

Response:

* success

---

# 5. DailyRewardController

## GET /rewards/daily/{userId}

Response:

* nextAvailableAt
* rewardType
* rewardAmount

## POST /rewards/daily/claim

Request:

* userId

Response:

* rewardedItems (coins, lives, powerups)

---

# 6. FriendController

## GET /friends/{userId}

Response:

* list of { friendId, username, avatarUrl, status }

## POST /friends/request

Request:

* requesterId
* receiverUsername

Response:

* success

## POST /friends/respond

Request:

* requestId
* action (ACCEPT, REJECT)

Response:

* success

---

# 7. ExternalFriendController

## GET /external-friends/{userId}

Response:

* list of { externalFriendId, isAppUser }

## POST /external-friends/sync

Request:

* userId
* provider
* externalIds[]

Response:

* success

---

# 8. InvitationController

## POST /invitation/send

Request:

* inviterId
* method
* target

Response:

* inviteCode

## POST /invitation/mark-joined

Request:

* inviteCode
* joinedUserId

Response:

* rewardGiven

---

# 9. LevelController

## POST /levels/attempt

Request:

* userId
* levelNumber

Response:

* success

## POST /levels/complete

Request:

* userId
* levelNumber
* stars
* usedPowerups
* score

Response:

* newTotalScore
* updatedProfile

---

# 10. AdController

## POST /ads/rewarded

Request:

* userId
* adId

Response:

* rewardedItems

## POST /ads/log

Request:

* userId
* adType

Response:

* success

---

# 11. PurchaseController

## GET /products

Response:

* list of product definitions

## POST /purchase/verify

Request:

* userId
* platform
* productId
* storeReceipt

Response:

* success
* grantedItems

---

# 12. EventController

## GET /events/active

Response:

* list of { id, name, description, startAt, endAt }

## POST /events/claim

Request:

* userId
* eventId

Response:

* rewardItems

---

# 13. LeaderboardController

## GET /leaderboard

Response:

* list of { userId, username, avatarUrl, rank, score }

## GET /leaderboard/{userId}

Response:

* userRank
* userScore

---

# 14. AdminController

## GET /admin/metrics

Response:

* list of metrics

## GET /admin/users

Response:

* list of all users (paginated)

## GET /admin/purchases

Response:

* purchase statistics

---

---

# Endpoint Summary (Names Only)

## AuthController

* register
* login
* guest
* oauth
* upgrade-guest

## UserController

* getUser
* updateAvatar

## ProfileController

* getProfile

## InventoryController

* getInventory
* consumeItem
* addItem

## DailyRewardController

* getDailyReward
* claimDailyReward

## FriendController

* getFriends
* sendFriendRequest
* respondFriendRequest

## ExternalFriendController

* getExternalFriends
* syncExternalFriends

## InvitationController

* sendInvitation
* markJoined

## LevelController

* levelAttempt
* levelComplete

## AdController

* rewardedAd
* logAd

## PurchaseController

* listProducts
* verifyPurchase

## EventController

* listActiveEvents
* claimEvent

## LeaderboardController

* getLeaderboard
* getUserLeaderboard

## AdminController

* getMetrics
* getUsers
* getPurchases
