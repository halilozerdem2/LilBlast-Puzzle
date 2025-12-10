# Lil Games — Backend Entities (Master Plan)

This document defines the full, fixed entity set for the Lil Games backend. These entities represent the domain model used across PostgreSQL, API DTOs, backend services, and the admin panel. Once approved, this structure will remain stable.

---

## 1. User

* id (UUID)
* authProvider (ENUM: GUEST, GOOGLE, FACEBOOK, EMAIL_PASSWORD)
* email (nullable for guest)
* username
* passwordHash (nullable for Google/Facebook/Guest)
* avatarUrl (nullable)
* createdAt
* lastLoginAt
* isGuest (boolean)

---

## 2. UserProfile

* userId (FK → User)
* totalAttempts
* totalWins
* winRatio (computed, not stored)
* threeStarCount
* totalPowerupsUsed
* mostUsedPowerup (ENUM: SHUFFLE, POWERSHUFFLE, MANIPULATE, DESTROY, NONE)
* totalScore
* lastLevelReached
* memberDurationDays (computed)

---

## 3. UserInventory

* userId (FK → User)
* coins
* lives
* shuffleCount
* powerShuffleCount
* manipulateCount
* destroyCount

---

## 4. DailyRewardCycle

* id (UUID)
* userId (FK → User)
* lastClaimedAt
* nextAvailableAt
* rewardType (ENUM)
* rewardAmount

---

## 5. Friend

* id (UUID)
* requesterId (FK → User)
* receiverId (FK → User)
* status (ENUM: PENDING, ACCEPTED, REJECTED, BLOCKED)
* createdAt

---

## 6. ExternalFriend

* id (UUID)
* userId (FK → User)
* externalProvider (ENUM: GOOGLE, FACEBOOK)
* externalFriendId (STRING)
* isAppUser (boolean)

---

## 7. Invitation

* id (UUID)
* inviterId (FK → User)
* inviteePhoneOrEmail
* inviteMethod (ENUM: FACEBOOK, WHATSAPP, LINK)
* inviteCode
* createdAt
* isJoined (boolean)
* joinedUserId (nullable FK → User)

---

## 8. LevelProgress

* id (UUID)
* userId (FK → User)
* levelNumber
* attempts
* wins
* highestStarsAchieved
* lastPlayedAt
* totalPowerupsUsedInThisLevel

---

## 9. AdActivity

* id (UUID)
* userId (FK → User)
* adType (ENUM: REWARDED, INTERSTITIAL, OFFERWALL)
* rewardGranted (boolean)
* createdAt

---

## 10. Purchase

* id (UUID)
* userId (FK → User)
* productId
* currencyType (ENUM: COINS, REALMONEY)
* platform (ENUM: ANDROID, IOS)
* amount
* createdAt

---

## 11. Product

* id (UUID)
* name
* description
* priceCoins
* priceRealMoney
* rewardCoins
* rewardLives
* rewardPowerups (JSON)
* isActive (boolean)

---

## 12. Event

* id (UUID)
* name
* description
* startAt
* endAt
* rewardCoins
* rewardLives
* rewardPowerups (JSON)
* isGlobal (boolean)

---

## 13. EventParticipation

* id (UUID)
* userId (FK → User)
* eventId (FK → Event)
* claimedAt (nullable)
* isClaimed (boolean)

---

## 14. LeaderboardSnapshot

* id (UUID)
* userId (FK → User)
* rank
* scoreMetric (ENUM: WINRATIO, TOTALSCORE, MIXED)
* snapshotAt

---

## 15. AdminUser

* id (UUID)
* email
* passwordHash
* role (ENUM: SUPERADMIN, ANALYST)
* createdAt

---

## 16. SystemMetric

* id (UUID)
* metricName
* metricValue
* createdAt
