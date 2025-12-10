# Lil Games — DTO Model

This document captures every request/response/data transfer object used by the backend so client applications (e.g., the Lil Blast game) can build strongly typed integrations. Field constraints reference the Jakarta validation annotations applied in code. Unless stated otherwise, IDs are UUID strings and timestamps are ISO-8601 instants.

## 1. Common Building Blocks

### `PowerupBundleDto`
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `shuffle` | int | `>= 0` | Shuffles granted.
| `powerShuffle` | int | `>= 0` | Power shuffles granted.
| `manipulate` | int | `>= 0` | Manipulate tokens granted.
| `destroy` | int | `>= 0` | Destroy tokens granted.

### `RewardItemsDto`
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `coins` | long | `>= 0` | Coin reward.
| `lives` | int | `>= 0` | Lives reward.
| `powerups` | `PowerupBundleDto` | required | Combined powerup rewards.

### `SuccessResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `success` | boolean | `true` indicates the mutation was applied.

## 2. AuthController DTOs

### `RegisterRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `email` | string | `@Email`, `@NotBlank` |
| `username` | string | `@NotBlank` |
| `password` | string | `@NotBlank` |

### `LoginRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `username` | string | `@NotBlank` |
| `password` | string | `@NotBlank` |

### `GuestAuthRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `deviceId` | string | `@NotBlank` |

### `OAuthRequest`
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `provider` | `AuthProvider` | `@NotNull` | `GOOGLE`, `FACEBOOK` |
| `oauthToken` | string | `@NotBlank` | Firebase token from the provider. |

### `UpgradeGuestRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `email` | string | `@Email`, `@NotBlank` |
| `username` | string | `@NotBlank` |
| `password` | string | `@NotBlank` |

### `AuthResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `userId` | UUID | Created/authenticated user identifier. |
| `authToken` | string | JWT access token used in the `Authorization` header. |

## 3. UserController DTOs

### `UserResponse`
| Field | Type |
| --- | --- |
| `userId` | UUID |
| `username` | string |
| `avatarUrl` | string (nullable) |
| `createdAt` | Instant |

### `UpdateAvatarRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `avatarUrl` | string | `@NotBlank` |

## 4. ProfileController DTOs

### `ProfileResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `totalAttempts` | long | Total level attempts. |
| `totalWins` | long | Completed levels. |
| `threeStarCount` | int | Levels finished with 3 stars. |
| `totalPowerupsUsed` | long | Aggregate powerups. |
| `mostUsedPowerup` | `PowerupType` | `SHUFFLE`, `POWERSHUFFLE`, `MANIPULATE`, `DESTROY`, `NONE`. |
| `totalScore` | long | Lifetime score. |
| `lastLevelReached` | int | Highest unlocked level. |

## 5. InventoryController DTOs

### `InventoryResponse`
| Field | Type |
| --- | --- |
| `coins` | long |
| `lives` | int |
| `shuffleCount` | int |
| `powerShuffleCount` | int |
| `manipulateCount` | int |
| `destroyCount` | int |

### `InventoryAdjustmentRequest` (used for consume/add)
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `userId` | UUID | `@NotNull` | Target player. |
| `itemType` | `InventoryItemType` | `@NotNull` | `COINS`, `LIVES`, `SHUFFLE`, `POWERSHUFFLE`, `MANIPULATE`, `DESTROY`. |
| `amount` | long | `@Min(1)` | Absolute delta to apply. |

## 6. DailyRewardController DTOs

### `DailyRewardResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `nextAvailableAt` | Instant | When the next claim is allowed. |
| `rewardType` | `RewardType` | Enum from entity canvas. |
| `rewardAmount` | long | Amount preview for the next claim. |

### `DailyRewardClaimRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |

### `DailyRewardClaimResponse`
| Field | Type |
| --- | --- |
| `rewardedItems` | `RewardItemsDto` |

## 7. FriendController DTOs

### `FriendRequestDto`
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `requesterId` | UUID | `@NotNull` |
| `receiverUsername` | string | `@NotBlank` | Username to search. |

### `FriendRespondRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `requestId` | UUID | `@NotNull` |
| `action` | `FriendResponseAction` | `@NotNull` (`ACCEPT`/`REJECT`) |

### `FriendSummaryDto`
| Field | Type | Notes |
| --- | --- | --- |
| `friendId` | UUID | Friend or request identifier. |
| `username` | string | Display name. |
| `avatarUrl` | string (nullable) | Latest avatar. |
| `status` | `FriendStatus` | `PENDING`, `ACCEPTED`, etc. |

## 8. ExternalFriendController DTOs

### `ExternalFriendSyncRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `provider` | `ExternalProvider` | `@NotNull` | `GOOGLE` or `FACEBOOK`. |
| `externalIds` | list<string> | `@NotEmpty` | Provider friend identifiers. |

### `ExternalFriendDto`
| Field | Type | Notes |
| --- | --- | --- |
| `externalFriendId` | string | Provider identifier. |
| `isAppUser` | boolean | `true` if friend already plays the game. |

## 9. InvitationController DTOs

### `SendInvitationRequest`
| Field | Type | Constraints | Notes |
| --- | --- | --- | --- |
| `inviterId` | UUID | `@NotNull` |
| `method` | `InviteMethod` | `@NotNull` (`FACEBOOK`, `WHATSAPP`, `LINK`) |
| `target` | string | `@NotBlank` | Phone/email/contact handle. |

### `SendInvitationResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `inviteCode` | string | Shareable referral code. |

### `MarkJoinedRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `inviteCode` | string | `@NotBlank` |
| `joinedUserId` | UUID | `@NotNull` |

### `MarkJoinedResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `rewardGiven` | boolean | `true` if inviter received referral rewards. |

## 10. LevelController DTOs

### `LevelAttemptRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `levelNumber` | int | `@Min(1)` |

### `LevelCompleteRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `levelNumber` | int | `@Min(1)` |
| `stars` | int | `@Min(0)` |
| `usedPowerups` | int | `@Min(0)` |
| `score` | long | `@Min(0)` |

### `LevelCompleteResponse`
| Field | Type |
| --- | --- |
| `newTotalScore` | long |
| `updatedProfile` | `ProfileResponse` |

## 11. AdController DTOs

### `RewardedAdRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `adId` | string | `@NotBlank` |

### `RewardedAdResponse`
| Field | Type |
| --- | --- |
| `rewardedItems` | `RewardItemsDto` |

### `AdLogRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `adType` | `AdType` | `@NotNull` | `REWARDED`, `INTERSTITIAL`, `OFFERWALL`. |

## 12. PurchaseController DTOs

### `ProductDto`
| Field | Type | Notes |
| --- | --- | --- |
| `id` | UUID | Product identifier. |
| `name` | string | Display label. |
| `description` | string | Optional marketing copy. |
| `priceCoins` | long | In-game price. |
| `priceRealMoney` | decimal | Real currency price. |
| `rewardCoins` | long | Coins gained after purchase. |
| `rewardLives` | int | Lives gained. |
| `rewardPowerups` | string (JSON) | Raw payload describing powerups. |
| `isActive` | boolean | Availability flag. |

### `PurchaseVerifyRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `platform` | `Platform` | `@NotNull` (`ANDROID`, `IOS`) |
| `productId` | UUID | `@NotNull` |
| `storeReceipt` | string | `@NotBlank` | Base64/JSON receipt blob. |

### `PurchaseVerifyResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `success` | boolean | `true` if verification passed. |
| `grantedItems` | `RewardItemsDto` | Empty when `success = false`. |

## 13. EventController DTOs

### `EventSummaryDto`
| Field | Type |
| --- | --- |
| `id` | UUID |
| `name` | string |
| `description` | string |
| `startAt` | Instant |
| `endAt` | Instant |

### `EventClaimRequest`
| Field | Type | Constraints |
| --- | --- | --- |
| `userId` | UUID | `@NotNull` |
| `eventId` | UUID | `@NotNull` |

### `EventClaimResponse`
| Field | Type |
| --- | --- |
| `rewardItems` | `RewardItemsDto` |

## 14. LeaderboardController DTOs

### `LeaderboardEntryDto`
| Field | Type |
| --- | --- |
| `userId` | UUID |
| `username` | string |
| `avatarUrl` | string (nullable) |
| `rank` | int |
| `score` | long |

### `LeaderboardResponse`
| Field | Type |
| --- | --- |
| `entries` | list<`LeaderboardEntryDto`> |

### `UserLeaderboardResponse`
| Field | Type |
| --- | --- |
| `userRank` | int |
| `userScore` | long |

## 15. AdminController DTOs

### `AdminMetricDto`
| Field | Type |
| --- | --- |
| `metricName` | string |
| `metricValue` | string |
| `createdAt` | Instant |

### `AdminMetricsResponse`
| Field | Type |
| --- | --- |
| `metrics` | list<`AdminMetricDto`> |

### `AdminUserSummary`
| Field | Type |
| --- | --- |
| `userId` | UUID |
| `email` | string |
| `username` | string |
| `createdAt` | Instant |

### `AdminUsersResponse`
| Field | Type | Notes |
| --- | --- | --- |
| `users` | list<`AdminUserSummary`> | Current page content. |
| `totalElements` | long | Result count. |
| `page` | int | 0-based page index. |
| `size` | int | Page size. |

### `AdminPurchaseStatsResponse`
| Field | Type |
| --- | --- |
| `totalPurchases` | long |
| `purchasesLast24Hours` | long |

## 16. Reward + Inventory Helpers

These DTOs appear in several controllers:
- `RewardItemsDto`, `DailyRewardClaimResponse`, `RewardedAdResponse`, `PurchaseVerifyResponse`, and `EventClaimResponse` share the same reward payload for consistency.
- `SuccessResponse` is used by any mutation returning only a boolean acknowledgement (e.g., avatar updates, inventory adjustments).

Use this document as the canonical contract when wiring the game or admin panel to the backend APIs.
