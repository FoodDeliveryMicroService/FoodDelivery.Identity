# Identity Service — Requirements Specification Document (RSD)

**Inspiration:** Talabat / Noon Food (smaller scale)

**Version:** 3.0

**Status:** Under Re-implementation (Clean Architecture)

**Date:** 2026-09-01

---

# 1. Introduction

## 1.1 Purpose

The Identity Service is the centralized authority for authentication, authorization, user account management, and customer delivery address management for every actor in the platform.

It manages:

- User identities
- Credentials
- Authentication
- JWT access and refresh tokens
- Roles and permissions
- User profiles
- Account status
- Customer saved delivery addresses
- Geographic reference data
- Location resolution for customer addresses

The Identity Service owns customer address data and geographic reference data used to support delivery location selection.

The service supports two methods for creating a delivery address:

1. **Current Location** — the customer’s device provides geographic coordinates, which are sent to the Identity Service for location resolution.
2. **Manual Selection** — the customer selects geographic data from cascading lookup values and enters the remaining address details.

---

## 1.2 Scope

### In Scope

- User registration and authentication
- JWT access tokens and refresh tokens
- Role-based access control (RBAC)
- User profile management
- Password reset and password change
- Account status management
- Customer saved address management
- Default address management
- Current location resolution
- Reverse geocoding integration
- Governorate and City lookup data
- Geographic lookup validation

### Out of Scope

- Restaurant data
- Menu data
- Order lifecycle management
- Payment processing
- Delivery agent assignment
- Real-time delivery tracking
- Notification content generation

The Identity Service does not create orders. The Order Service consumes customer address information through internal service-to-service APIs.

---

# 2. Actors / Roles

| Role | Description |
| --- | --- |
| Customer | End user who browses restaurants, manages delivery addresses, and places orders |
| Restaurant Owner | Manages one or more restaurants and restaurant-related operations |
| Delivery Agent | Fulfills deliveries (future phase) |
| System Admin | Has administrative access to users, roles, account status, and audit information |

---

# 3. Functional Requirements

## FR-01 — Register

### Inputs

- Name
- Email
- Phone Number
- Password
- Role

Allowed registration roles:

- Customer
- RestaurantOwner

### Acceptance Criteria

- Given a valid registration payload with a unique email, when submitted, then a new user account is created with the requested role and `Active` status.
- Given an email that already exists in the system, when submitted, then the API returns `409 Conflict`.
- Given a password that does not meet the password complexity policy, when submitted, then the API returns `400 Bad Request`.
- Given a valid registration, when completed, then the response returns the created user identifier and a success message.
- A token is not issued during registration unless explicitly enabled by future authentication requirements.

---

## FR-02 — Confirm Email

**As a** new user, **I want to** confirm my email address using a 6-digit verification code sent to my inbox, **so that** I can activate my account and access all platform features securely.

---

## **Acceptance Criteria**

| **ID** | **Criterion** |
| --- | --- |
| AC-01 | After registration, a 6-digit numeric confirmation code is generated and sent to the user's email |
| AC-02 | The confirmation code expires after 15 minutes |
| AC-03 | The user can request a new confirmation code if the previous one expired |
| AC-04 | The user submits the code to confirm their email address |
| AC-05 | Only one confirmation code can be active per user at a time |
| AC-06 | After successful confirmation, the user's `EmailConfirmed` flag is set to `true` |
| AC-07 | The confirmation code is invalidated after use or expiration |
| AC-08 | Rate limiting prevents brute-force attempts (max 5 attempts per minute) |

## FR-02 — Login

### Acceptance Criteria

- Given correct email and password, when submitted, then the API returns an access token and a refresh token.
- Given incorrect credentials, when submitted, then the API returns `401 Unauthorized`.
- The response must not indicate whether the email or password was incorrect.
- Given a suspended account, when login is attempted, then the API returns `403 Forbidden`.
- Given a successful login, then the JWT contains:
    - UserId
    - Email
    - Name
    - Role
    - Standard JWT claims including issuer and expiration information.

### Post-Login Address Behavior

After successful authentication, the client may retrieve the customer’s saved addresses.

The client determines the initial delivery location as follows:

1. If the customer has a default saved address, that address may be used as the active delivery location.
2. If the customer has saved addresses but no explicitly selected address in the client session, the default address is used.
3. If the customer has no saved addresses, the client should direct the customer to set a delivery location before checkout.

The Identity Service does not automatically access the customer’s device location during login.

Location permission must be requested by the client application.

---

## FR-03 — Refresh Token

### Acceptance Criteria

- Given a valid, non-expired, non-revoked refresh token, when exchanged, then a new access token and a rotated refresh token are issued.
- Given an expired, revoked, or invalid refresh token, then the API returns `401 Unauthorized`.
- Given a refresh token is successfully exchanged, then the previous refresh token is revoked immediately.
- Refresh tokens follow a single-use rotation policy.

---

## FR-04 — Logout

### Acceptance Criteria

- Given an authenticated user with an active refresh token, when logout is requested, then the refresh token is revoked.
- A revoked refresh token cannot be used to obtain a new access token.
- Given a revoked token is used again, then the API returns `401 Unauthorized`.

---

## FR-05 — Role and Permission Management

### Acceptance Criteria

- Only authorized administrators can assign or modify user roles.
- Given a role change is successfully applied, future issued JWT tokens reflect the new role.
- Existing tokens are not retroactively modified.
- Given a non-admin attempts to modify user roles, then the API returns `403 Forbidden`.
- Role changes are recorded in audit logs.

---

## FR-06 — Profile Management

### Acceptance Criteria

- An authenticated user can retrieve their own profile.
- An authenticated user can update their own:
    - Name
    - Phone Number
    - Profile Picture
- A user cannot modify another user’s profile.
- An administrator may update user information according to administrative permissions.
- Administrative changes are audit logged.

---

## FR-07 — Password Reset

### Acceptance Criteria

- Given a password reset request for a registered email, then a time-limited reset token is generated.
- The reset process is integrated with the Notification Service.
- Given a valid reset token and a valid new password, then the password is updated.
- A successfully consumed reset token cannot be reused.
- Given an expired or invalid reset token, then the API returns `400 Bad Request`.

---

## FR-08 — Account Status Management

### Acceptance Criteria

- An administrator can suspend a user account.
- A suspended user cannot log in.
- A suspended user cannot obtain new access tokens using refresh token flows.
- An administrator can reactivate a suspended account.
- A reactivated user can authenticate normally.
- Non-admin users cannot modify account status.

---

# 4. Customer Delivery Address Management

## 4.1 Core Principle

Customer delivery addresses are owned and managed by the Identity Service.

A customer may have multiple saved addresses, for example:

- Home
- Work
- Parents
- Other

One saved address may be marked as the customer’s default address.

Addresses are created independently from order checkout and are reused across multiple orders.

The Order Service must not accept raw address fields directly during checkout.

Instead, the customer selects a saved `AddressId`.

---

## 4.2 Address Creation Methods

The system supports two methods for determining the geographic location of an address:

### Method A — Current Location

The client application requests permission to access the customer’s current location.

The device or browser determines the geographic coordinates and provides:

- Latitude
- Longitude
- Optional accuracy information

The frontend sends the coordinates to the Identity Service.

The Identity Service resolves the geographic location through a reverse geocoding provider.

The resolved location is matched against internal lookup data.

The customer then reviews or completes the address details before saving.

---

### Method B — Manual Location Selection

The customer manually selects:

1. Governorate
2. City

The City list is filtered according to the selected Governorate.

The customer then enters detailed address information.

---

## 4.3 Address Fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| Id | UUID | Auto | Primary key |
| CustomerId | UUID | Yes | Owner of the address |
| Label | String | Yes | Home, Work, etc. |
| GovernorateId | UUID | Yes | Selected or resolved governorate |
| CityId | UUID | Yes | Selected or resolved city |
| Street | String | Yes | Street name or detailed street description |
| BuildingNumber | String | Yes | Building or house number |
| Floor | String | No | Floor number |
| Apartment | String | No | Apartment, unit, or office number |
| Landmark | String | No | Nearby recognizable location |
| Latitude | Decimal | Recommended | Geographic latitude |
| Longitude | Decimal | Recommended | Geographic longitude |
| IsDefault | Boolean | No | Indicates the customer’s default address |
| CreatedAt | DateTime | Auto | Address creation timestamp |
| UpdatedAt | DateTime | Auto | Last modification timestamp |

`Floor` and `Apartment` are optional because not all delivery locations are apartments.

Examples include:

- Villas
- Houses
- Ground-floor shops
- Offices
- Commercial locations

---

# 5. Current Location Resolution

## FR-09 — Resolve Current Location

### Description

The client obtains the customer’s current geographic coordinates and sends them to the Identity Service.

The Identity Service performs reverse geocoding to convert coordinates into geographic information.

### Input

- Latitude
- Longitude

Optional:

- Accuracy

### Processing Flow

```
Customer Device
      ↓
Browser / Client Geolocation API
      ↓
Latitude + Longitude
      ↓
Frontend
      ↓
Identity Service
      ↓
Reverse Geocoding Provider
      ↓
Governorate / City / Address Components
      ↓
Match with Internal Lookup Data
      ↓
Return Resolved Location
```

### Acceptance Criteria

- Given valid latitude and longitude coordinates, when location resolution is requested, then the Identity Service attempts to resolve the geographic location.
- The Identity Service calls an external reverse geocoding provider through an infrastructure abstraction.
- The resolved governorate is matched against the internal Governorate lookup table.
- The resolved city is matched against cities belonging to the resolved governorate.
- Given successful matching, then the API returns:
    - Latitude
    - Longitude
    - Governorate
    - GovernorateId
    - City
    - CityId
    - Any available geographic details returned by the provider
- Given the external provider cannot resolve a valid supported location, then the API returns an appropriate failure response or unresolved result.
- The customer must be allowed to manually modify the resolved Governorate and City before saving the address.
- Current location resolution does not automatically save an address.

---

## FR-10 — Add Address Using Current Location

### Acceptance Criteria

- Given a customer has resolved their current location, when they provide the required address details, then a new address can be saved.
- The address must contain a valid Governorate and City.
- Latitude and Longitude returned from the location process are stored with the address.
- The customer can modify the resolved geographic values before saving.
- If the selected City does not belong to the selected Governorate, the API returns `400 Bad Request`.
- If `IsDefault = true`, then any existing default address belonging to that customer is unset.
- The saved address is linked only to the authenticated customer.

---

# 6. Manual Address Management

## FR-11 — Add Address Manually

### Input

- Label
- GovernorateId
- CityId
- Street
- BuildingNumber
- Floor
- Apartment
- Landmark
- Optional Latitude
- Optional Longitude
- IsDefault

### Acceptance Criteria

- Given a valid authenticated customer and valid address data, when submitted, then the address is saved.
- Given the selected City does not belong to the selected Governorate, then the API returns `400 Bad Request`.
- Given the address is marked as default, then any existing default address for that customer is unset.
- A customer can only create addresses associated with their own account.
- The system may enforce a configurable maximum number of saved addresses.
- Given the maximum number of addresses is reached, then the API returns `409 Conflict`.

---

## FR-12 — Update Address

### Acceptance Criteria

- Given an authenticated customer updates one of their own addresses, then the updated values are persisted.
- A customer cannot update another customer’s address.
- Given the address does not belong to the authenticated customer, then the API returns `404 Not Found` or `403 Forbidden` according to the selected ownership policy.
- Geographic validation must still ensure that the selected City belongs to the selected Governorate.

---

## FR-13 — Delete Address

### Acceptance Criteria

- An authenticated customer can delete their own saved address.
- A customer cannot delete another customer’s address.
- Address deletion may use soft delete or hard delete according to implementation configuration.
- Historical orders are not affected by address deletion because orders store address snapshots.

---

## FR-14 — List Saved Addresses

### Acceptance Criteria

- Given an authenticated customer requests their addresses, then all active saved addresses belonging to that customer are returned.
- The response includes:
    - Address details
    - Resolved Governorate name
    - Resolved City name
    - Geographic coordinates when available
    - Default address status
- If the customer has no saved addresses, the API returns `200 OK` with an empty collection.

---

## FR-15 — Get Single Address

### Acceptance Criteria

- An authenticated customer can retrieve one of their own addresses.
- A customer cannot retrieve another customer’s address.
- The response includes the full address details.

---

## FR-16 — Set Default Address

### Acceptance Criteria

- Given an authenticated customer selects one of their addresses as default, then that address becomes the customer’s default address.
- Any previous default address is unset.
- Only one default address may exist per customer.
- A customer cannot set another customer’s address as default.

---

## FR-17 — Resolve Address by ID (Internal API)

### Description

This endpoint is intended for internal service-to-service communication.

The Order Service uses this endpoint during checkout to validate an `AddressId` and retrieve the complete address information.

### Acceptance Criteria

- Given a valid AddressId belonging to the specified customer, then the full address is returned.
- The returned address includes:
    - Address label
    - Governorate name
    - City name
    - Street
    - Building number
    - Floor
    - Apartment
    - Landmark
    - Latitude
    - Longitude
- Given the address does not exist, then the API returns `404 Not Found`.
- Given the address does not belong to the specified customer, then the API rejects the request.
- This endpoint is protected for authorized internal services.

---

# 7. Geographic Lookup Management

## 7.1 Core Principle

Governorates and Cities are reference data owned by the Identity Service.

They are used for:

- Manual address creation
- Geographic validation
- Current location matching
- Address display

They are not normally managed by customers.

---

## 7.2 Geographic Hierarchy

```
Governorate
      ↓
City
```

Each City belongs to exactly one Governorate.

---

## 7.3 Governorates Data Model

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| Name | String | Governorate name |
| NormalizedName | String | Normalized value used for matching external provider results |

`NormalizedName` may be used internally to improve matching between reverse geocoding provider results and lookup values.

---

## 7.4 Cities Data Model

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| GovernorateId | UUID | Parent Governorate |
| Name | String | City name |
| NormalizedName | String | Normalized value used for provider matching |

---

## FR-18 — List Governorates

### Acceptance Criteria

- Given a request for Governorate lookup data, then all supported Governorates are returned.
- Results are ordered according to the configured display ordering.
- The endpoint may be publicly accessible or authenticated depending on application requirements.

---

## FR-19 — List Cities by Governorate

### Acceptance Criteria

- Given a valid GovernorateId, then all Cities belonging to that Governorate are returned.
- Given an invalid GovernorateId, then the API returns `404 Not Found`.
- Results are returned according to the configured display ordering.

---

## FR-20 — Validate Geographic Hierarchy

### Acceptance Criteria

- A City must belong to exactly one Governorate.
- An address cannot reference a City belonging to a different Governorate.
- Geographic relationships are validated before an address is created or updated.

---

# 8. Reverse Geocoding Integration

## 8.1 Responsibility

The frontend is responsible for obtaining geographic coordinates from the user’s device.

The Identity Service is responsible for resolving those coordinates through an external reverse geocoding provider.

The backend does not directly access the customer’s device GPS hardware.

---

## 8.2 Architecture

The Application Layer must not depend directly on a specific maps provider.

An abstraction is used:

```
Application Layer
       │
       ▼
ILocationResolver
       │
       ▼
Infrastructure Layer
       │
       ├── Provider A Implementation
       │
       ├── Provider B Implementation
       │
       └── Future Provider Implementation
```

Example abstraction:

```csharp
public interface ILocationResolver
{
    Task<ResolvedLocation> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken);
}
```

The Application Layer depends only on the abstraction.

The Infrastructure Layer contains the external provider implementation.

---

## 8.3 Provider Result Processing

The reverse geocoding provider may return geographic information such as:

- Country
- Administrative area
- Governorate
- City
- District
- Street
- Formatted address

The Identity Service maps supported geographic information to its internal lookup data.

The provider result is not treated as the authoritative identifier for internal geographic entities.

Internal `GovernorateId` and `CityId` values remain authoritative.

---

# 9. Business Rules

| ID | Rule |
| --- | --- |
| BR-01 | Email must be unique across the platform |
| BR-02 | A suspended account cannot log in or obtain new tokens |
| BR-03 | Refresh tokens are single-use and rotate when exchanged |
| BR-04 | Role changes affect newly issued tokens |
| BR-05 | Customers can manage only their own addresses |
| BR-06 | Only one default address may exist per customer |
| BR-07 | A City must belong to exactly one Governorate |
| BR-08 | An address must reference a valid Governorate and City |
| BR-09 | Current device location permission is requested by the client, not the backend |
| BR-10 | The backend receives geographic coordinates but does not directly access device GPS hardware |
| BR-11 | Reverse geocoding results must be mapped to internal lookup entities before saving an address |
| BR-12 | Customers may manually correct geographic data resolved from current location |
| BR-13 | Floor and Apartment fields are optional |
| BR-14 | Saved addresses belong exclusively to Customer accounts |
| BR-15 | Orders must not depend on mutable live address data after creation |

---

# 10. Data Model

## 10.1 Users

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| Name | String | Full name |
| Email | String | Unique email |
| PhoneNumber | String | Customer contact number |
| PasswordHash | String | Hashed password |
| Role | Enum | Customer, RestaurantOwner, DeliveryAgent, Admin |
| Status | Enum | Active, Suspended |
| ProfilePictureUrl | String | Optional profile picture |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Last update timestamp |

---

## 10.2 Refresh Tokens

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| UserId | UUID | User owner |
| TokenHash | String | Hashed refresh token |
| ExpiresAt | DateTime | Expiration timestamp |
| IsRevoked | Boolean | Revocation status |
| CreatedAt | DateTime | Creation timestamp |
| ReplacedByTokenId | UUID | Replacement token reference |

---

## 10.3 Password Reset Tokens

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| UserId | UUID | User owner |
| TokenHash | String | Hashed reset token |
| ExpiresAt | DateTime | Expiration timestamp |
| IsUsed | Boolean | Indicates token consumption |
| CreatedAt | DateTime | Creation timestamp |

---

## 10.4 Addresses

| Field | Type | Description |
| --- | --- | --- |
| Id | UUID | Primary key |
| CustomerId | UUID | Address owner |
| Label | String | Home, Work, etc. |
| GovernorateId | UUID | Geographic reference |
| CityId | UUID | Geographic reference |
| Street | String | Street information |
| BuildingNumber | String | Building or house number |
| Floor | String | Optional |
| Apartment | String | Optional |
| Landmark | String | Optional |
| Latitude | Decimal | Geographic latitude |
| Longitude | Decimal | Geographic longitude |
| IsDefault | Boolean | Default address indicator |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime | Modification timestamp |

---

# 11. Integration with Other Services

| Service | Purpose |
| --- | --- |
| API Gateway | Routes requests and validates incoming authentication flows |
| Order Service | Resolves customer addresses by AddressId during checkout |
| Restaurant Service | Verifies restaurant owner identity for protected operations |
| Payment Service | Verifies customer identity for payment-related operations |
| Notification Service | Handles password reset and account-related notifications |

---

# 12. API Endpoints Summary

## 12.1 Authentication

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/auth/register` | Register a new user |
| POST | `/auth/login` | Authenticate and receive tokens |
| POST | `/auth/refresh` | Exchange refresh token |
| POST | `/auth/logout` | Logout and revoke refresh token |
| POST | `/auth/password-reset-request` | Request password reset |
| POST | `/auth/password-reset` | Reset password |

---

## 12.2 Profile

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/profile` | Get current user profile |
| PUT | `/profile` | Update current user profile |

---

## 12.3 Address Management

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/addresses` | Add new address |
| GET | `/addresses` | List my addresses |
| GET | `/addresses/{id}` | Get single address |
| PUT | `/addresses/{id}` | Update address |
| DELETE | `/addresses/{id}` | Delete address |
| PUT | `/addresses/{id}/set-default` | Set default address |

---

## 12.4 Location Resolution

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/locations/resolve` | Resolve Latitude/Longitude into supported geographic data |

Example request:

```json
{
  "latitude": 30.123456,
  "longitude": 31.123456
}
```

Example response:

```json
{
  "latitude": 30.123456,
  "longitude": 31.123456,
  "governorate": {
    "id": "governorate-id",
    "name": "Sharqia"
  },
  "city": {
    "id": "city-id",
    "name": "Zagazig"
  },
  "street": null,
  "formattedAddress": null
}
```

The customer reviews and completes the address before it is saved.

---

## 12.5 Geographic Lookups

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/lookups/governorates` | List Governorates |
| GET | `/lookups/cities/{governorateId}` | List Cities belonging to a Governorate |

---

## 12.6 Internal Service APIs

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/internal/users/{id}` | Resolve user information |
| GET | `/internal/addresses/{id}` | Resolve saved address |
| GET | `/internal/users/{id}/validate` | Validate user existence and account status |

Internal APIs require service-to-service authentication.

---

# 13. Order Service Integration

At checkout, the Order Service receives:

- RestaurantId
- Order items
- Selected `AddressId`
- Optional order notes

The Order Service must not receive raw delivery address fields.

The Order Service calls the Identity Service to:

1. Validate that the address exists.
2. Validate that the address belongs to the authenticated customer.
3. Retrieve the full address details.

The Order Service then creates an immutable address snapshot.

Future changes to the customer’s saved address must not modify historical orders.

---

# 14. Non-Functional Requirements

| Category | Requirement |
| --- | --- |
| Security | Passwords are securely hashed; refresh tokens are stored securely; HTTPS is required |
| Authentication | JWT-based authentication and authorization |
| Performance | Lookup endpoints should provide low-latency responses |
| Scalability | Stateless service design supports horizontal scaling |
| Reliability | External geocoding failures must not corrupt address data |
| Data Integrity | City/Governorate relationships are validated |
| Privacy | Device location is accessed only after client-side user permission |
| Auditability | Security-sensitive account actions are logged |
| Extensibility | Reverse geocoding providers can be replaced through Infrastructure implementations |

---

# 15. Overall Acceptance Criteria

| ID | Criterion |
| --- | --- |
| AC-01 | Users can register and authenticate securely |
| AC-02 | Login issues access and refresh tokens |
| AC-03 | Customers can manage multiple saved delivery addresses |
| AC-04 | Only one default address can exist per customer |
| AC-05 | Customers can use current device location to begin address creation |
| AC-06 | Geographic coordinates are obtained by the client and sent to the backend |
| AC-07 | The backend resolves coordinates through a reverse geocoding abstraction |
| AC-08 | Resolved geographic values are matched against internal Governorate and City lookup data |
| AC-09 | Customers can manually select Governorate and City as an alternative |
| AC-10 | Floor and Apartment are optional address fields |
| AC-11 | A City must belong to the selected Governorate |
| AC-12 | Customers can correct automatically resolved geographic information |
| AC-13 | The Order Service receives only AddressId during checkout |
| AC-14 | Historical orders store immutable address snapshots |
| AC-15 | External location providers can be replaced without changing Application Layer business logic |
