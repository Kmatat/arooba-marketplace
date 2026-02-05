# Arooba Marketplace - API Reference

> Detailed API documentation with request/response examples for the Arooba Marketplace backend.

**Base URL**: `https://localhost:5001/api` (Development) | `https://api.aroobh.com/api` (Production)

**Authentication**: All endpoints (except `/auth/register` and `/auth/login`) require a valid JWT Bearer token in the `Authorization` header.

```
Authorization: Bearer <your-jwt-token>
```

---

## Table of Contents

- [Authentication](#authentication)
- [Vendors](#vendors)
- [Products](#products)
- [Orders](#orders)
- [Pricing Engine](#pricing-engine)
- [Finance](#finance)
- [Shipping](#shipping)
- [Customers](#customers)
- [Dashboard](#dashboard)
- [Categories](#categories)
- [Error Responses](#error-responses)

---

## Authentication

### POST /api/auth/register

Register a new user account.

**Request Body**:

```json
{
  "mobileNumber": "+201234567890",
  "fullName": "Ahmed Hassan",
  "fullNameAr": "احمد حسن",
  "email": "ahmed@example.com",
  "password": "SecureP@ss123",
  "confirmPassword": "SecureP@ss123",
  "role": "customer"
}
```

**Response** `201 Created`:

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "mobileNumber": "+201234567890",
  "fullName": "Ahmed Hassan",
  "role": "customer",
  "isVerified": false,
  "createdAt": "2024-03-15T10:30:00Z"
}
```

**Validation Rules**:
- `mobileNumber`: Required, must start with `+20`, 13 characters total
- `fullName`: Required, 2-100 characters
- `password`: Required, minimum 8 characters, must contain uppercase, lowercase, digit, and special character
- `role`: Must be one of `customer`, `parent_vendor`

---

### POST /api/auth/login

Authenticate and receive JWT tokens.

**Request Body**:

```json
{
  "mobileNumber": "+201234567890",
  "password": "SecureP@ss123"
}
```

**Response** `200 OK`:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "d4e5f6a7-b8c9-0123-4567-890abcdef123",
  "expiresAt": "2024-03-15T11:30:00Z",
  "user": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "fullName": "Ahmed Hassan",
    "role": "customer"
  }
}
```

---

## Vendors

### POST /api/vendors

Register a new vendor. Requires `parent_vendor` role or admin.

**Request Body**:

```json
{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "businessName": "Nile Crafts",
  "businessNameAr": "حرف النيل",
  "type": "legalized",
  "commercialRegNumber": "CR-12345-2024",
  "taxId": "TAX-98765-EG",
  "isVatRegistered": true,
  "defaultCommissionRate": 0.20,
  "bankName": "Commercial International Bank (CIB)",
  "bankAccountNumber": "EG380019000500000001234567891",
  "pickupLocations": [
    {
      "label": "Main Workshop",
      "address": "15 El-Moez Street, Islamic Cairo",
      "city": "Cairo",
      "zoneId": "cairo",
      "gpsLat": 30.0459,
      "gpsLng": 31.2619,
      "contactName": "Ahmed Hassan",
      "contactPhone": "+201234567890",
      "isDefault": true
    }
  ]
}
```

**Response** `201 Created`:

```json
{
  "id": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "businessName": "Nile Crafts",
  "businessNameAr": "حرف النيل",
  "type": "legalized",
  "status": "pending",
  "isVatRegistered": true,
  "commercialRegNumber": "CR-12345-2024",
  "taxId": "TAX-98765-EG",
  "defaultCommissionRate": 0.20,
  "reliabilityStrikes": 0,
  "averageRating": 0.0,
  "totalOrders": 0,
  "totalRevenue": 0.0,
  "subVendorIds": [],
  "pickupLocations": [
    {
      "id": "pl-a1b2c3d4-e5f6-7890",
      "label": "Main Workshop",
      "address": "15 El-Moez Street, Islamic Cairo",
      "city": "Cairo",
      "zoneId": "cairo",
      "isDefault": true
    }
  ],
  "createdAt": "2024-03-15T10:30:00Z",
  "updatedAt": "2024-03-15T10:30:00Z"
}
```

**Validation Rules**:
- `businessName`: Required, 2-200 characters
- `type`: Must be `legalized` or `non_legalized`
- `commercialRegNumber`: Required if `type` is `legalized`
- `taxId`: Required if `type` is `legalized`
- `defaultCommissionRate`: Required, between 0.0 and 1.0
- At least one pickup location required

---

### GET /api/vendors

List all vendors with pagination and filtering.

**Query Parameters**:

| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| `page` | int | Page number | 1 |
| `pageSize` | int | Items per page | 20 |
| `status` | string | Filter by status | (all) |
| `type` | string | Filter by vendor type | (all) |
| `search` | string | Search by business name | (none) |
| `sortBy` | string | Sort field | `createdAt` |
| `sortOrder` | string | `asc` or `desc` | `desc` |

**Response** `200 OK`:

```json
{
  "items": [
    {
      "id": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "businessName": "Nile Crafts",
      "businessNameAr": "حرف النيل",
      "type": "legalized",
      "status": "active",
      "isVatRegistered": true,
      "averageRating": 4.7,
      "totalOrders": 142,
      "totalRevenue": 285000.00,
      "subVendorCount": 3,
      "createdAt": "2024-03-15T10:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 87,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### POST /api/vendors/{vendorId}/sub-vendors

Add a sub-vendor under a parent vendor.

**Request Body**:

```json
{
  "internalName": "Aunt Nadia",
  "internalNameAr": "خالتي نادية",
  "defaultLeadTimeDays": 7,
  "upliftType": "percentage",
  "upliftValue": 0.10
}
```

**Response** `201 Created`:

```json
{
  "id": "sv-a1b2c3d4-e5f6-7890",
  "parentVendorId": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "internalName": "Aunt Nadia",
  "internalNameAr": "خالتي نادية",
  "defaultLeadTimeDays": 7,
  "upliftType": "percentage",
  "upliftValue": 0.10,
  "isActive": true,
  "createdAt": "2024-03-15T12:00:00Z"
}
```

---

## Products

### POST /api/products

Create a new product. The pricing engine automatically calculates the final price.

**Request Body**:

```json
{
  "parentVendorId": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "subVendorId": "sv-a1b2c3d4-e5f6-7890",
  "categoryId": "home-decor-fragile",
  "title": "Hand-Painted Ceramic Vase",
  "titleAr": "مزهرية خزفية مرسومة يدويا",
  "description": "Authentic Egyptian ceramic vase, hand-painted with traditional Pharaonic motifs. Each piece is unique.",
  "descriptionAr": "مزهرية خزفية مصرية أصيلة، مرسومة يدويا بزخارف فرعونية تقليدية. كل قطعة فريدة.",
  "images": [
    "https://cdn.aroobh.com/products/vase-front.jpg",
    "https://cdn.aroobh.com/products/vase-detail.jpg"
  ],
  "costPrice": 150.00,
  "sellingPrice": 400.00,
  "pickupLocationId": "pl-a1b2c3d4-e5f6-7890",
  "stockMode": "ready_stock",
  "quantityAvailable": 25,
  "weightKg": 1.5,
  "dimensionL": 20,
  "dimensionW": 20,
  "dimensionH": 35,
  "isLocalOnly": false
}
```

**Response** `201 Created`:

The response includes the auto-calculated pricing breakdown from the pricing engine:

```json
{
  "id": "p-a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "sku": "NILCR-NADIA-HDCF-001",
  "parentVendorId": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "subVendorId": "sv-a1b2c3d4-e5f6-7890",
  "categoryId": "home-decor-fragile",
  "title": "Hand-Painted Ceramic Vase",
  "titleAr": "مزهرية خزفية مرسومة يدويا",
  "costPrice": 150.00,
  "sellingPrice": 400.00,
  "pricingBreakdown": {
    "vendorBasePrice": 400.00,
    "cooperativeFee": 0.00,
    "parentVendorUplift": 40.00,
    "marketplaceUplift": 100.00,
    "logisticsSurcharge": 10.00,
    "vendorVat": 61.60,
    "aroobaVat": 15.40,
    "finalPrice": 627.00,
    "bucketA_vendorRevenue": 440.00,
    "bucketB_vendorVat": 61.60,
    "bucketC_aroobaRevenue": 110.00,
    "bucketD_aroobaVat": 15.40
  },
  "finalPrice": 627.00,
  "pickupLocationId": "pl-a1b2c3d4-e5f6-7890",
  "stockMode": "ready_stock",
  "quantityAvailable": 25,
  "weightKg": 1.5,
  "dimensionL": 20,
  "dimensionW": 20,
  "dimensionH": 35,
  "volumetricWeight": 2.80,
  "isLocalOnly": false,
  "status": "pending_review",
  "isFeatured": false,
  "createdAt": "2024-03-15T14:00:00Z",
  "updatedAt": "2024-03-15T14:00:00Z"
}
```

**Pricing Calculation Notes** (for the above product):
- Vendor base price: 400.00 EGP
- Vendor is VAT-registered and legalized, so cooperative fee = 0
- Sub-vendor has 10% parent uplift: 400 x 0.10 = 40.00 EGP
- Category `home-decor-fragile` default uplift is 25%: 400 x 0.25 = 100.00 EGP
- Logistics surcharge: 10.00 EGP
- Bucket A (Vendor Revenue): 400 + 40 = 440.00 EGP
- Bucket B (Vendor VAT): 440 x 0.14 = 61.60 EGP
- Bucket C (Arooba Revenue): 0 + 100 + 10 = 110.00 EGP
- Bucket D (Arooba VAT): 110 x 0.14 = 15.40 EGP
- Final Price: 440 + 61.60 + 110 + 15.40 = 627.00 EGP

---

### GET /api/products

List products with pagination, filtering, and search.

**Query Parameters**:

| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| `page` | int | Page number | 1 |
| `pageSize` | int | Items per page | 20 |
| `categoryId` | string | Filter by category | (all) |
| `vendorId` | guid | Filter by vendor | (all) |
| `status` | string | Filter by status | (all) |
| `minPrice` | decimal | Minimum final price | (none) |
| `maxPrice` | decimal | Maximum final price | (none) |
| `search` | string | Search title / description | (none) |
| `zoneId` | string | Filter by delivery zone | (none) |
| `stockMode` | string | `ready_stock` or `made_to_order` | (all) |
| `sortBy` | string | Sort field (`finalPrice`, `createdAt`, `rating`) | `createdAt` |
| `sortOrder` | string | `asc` or `desc` | `desc` |

---

## Orders

### POST /api/orders

Create a new order. The system automatically splits items into shipments by pickup location and calculates the 5-bucket waterfall for each item.

**Request Body**:

```json
{
  "customerId": "c-a1b2c3d4-e5f6-7890",
  "items": [
    {
      "productId": "p-a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "quantity": 2
    },
    {
      "productId": "p-b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "quantity": 1
    }
  ],
  "deliveryAddress": "42 Tahrir Square, Downtown",
  "deliveryCity": "Cairo",
  "deliveryZoneId": "cairo",
  "paymentMethod": "cod"
}
```

**Response** `201 Created`:

```json
{
  "id": "ord-a1b2c3d4-e5f6-7890",
  "customerId": "c-a1b2c3d4-e5f6-7890",
  "customerName": "Ahmed Hassan",
  "items": [
    {
      "id": "oi-001",
      "productId": "p-a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "productTitle": "Hand-Painted Ceramic Vase",
      "productImage": "https://cdn.aroobh.com/products/vase-front.jpg",
      "vendorId": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "vendorName": "Nile Crafts",
      "quantity": 2,
      "unitPrice": 627.00,
      "totalPrice": 1254.00,
      "pickupLocationId": "pl-a1b2c3d4-e5f6-7890",
      "bucketA_vendorRevenue": 880.00,
      "bucketB_vendorVat": 123.20,
      "bucketC_aroobaRevenue": 220.00,
      "bucketD_aroobaVat": 30.80,
      "bucketE_logisticsFee": 0.00
    },
    {
      "id": "oi-002",
      "productId": "p-b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "productTitle": "Silver Ankh Necklace",
      "productImage": "https://cdn.aroobh.com/products/ankh.jpg",
      "vendorId": "v2c3d4e5-f6a7-8901-bcde-f12345678901",
      "vendorName": "Cairo Silver",
      "quantity": 1,
      "unitPrice": 460.00,
      "totalPrice": 460.00,
      "pickupLocationId": "pl-b2c3d4e5-f6a7",
      "bucketA_vendorRevenue": 350.00,
      "bucketB_vendorVat": 49.00,
      "bucketC_aroobaRevenue": 52.50,
      "bucketD_aroobaVat": 8.50,
      "bucketE_logisticsFee": 0.00
    }
  ],
  "subtotal": 1714.00,
  "totalDeliveryFee": 95.00,
  "totalAmount": 1809.00,
  "paymentMethod": "cod",
  "deliveryAddress": "42 Tahrir Square, Downtown",
  "deliveryCity": "Cairo",
  "deliveryZoneId": "cairo",
  "shipments": [
    {
      "id": "shp-001",
      "orderId": "ord-a1b2c3d4-e5f6-7890",
      "pickupLocationId": "pl-a1b2c3d4-e5f6-7890",
      "deliveryFee": 55.00,
      "codAmountDue": 1309.00,
      "status": "pending",
      "estimatedDeliveryDate": "2024-03-17T18:00:00Z"
    },
    {
      "id": "shp-002",
      "orderId": "ord-a1b2c3d4-e5f6-7890",
      "pickupLocationId": "pl-b2c3d4e5-f6a7",
      "deliveryFee": 40.00,
      "codAmountDue": 500.00,
      "status": "pending",
      "estimatedDeliveryDate": "2024-03-17T18:00:00Z"
    }
  ],
  "status": "pending",
  "createdAt": "2024-03-15T16:00:00Z",
  "updatedAt": "2024-03-15T16:00:00Z"
}
```

**Key behaviors**:
- Items are grouped by `pickupLocationId` and each group becomes a separate `Shipment`
- The 5-bucket waterfall is calculated per line item
- COD amount per shipment is the sum of item prices plus the delivery fee for that shipment
- Delivery fee is calculated per shipment based on zone-to-zone rates and chargeable weight

---

### PATCH /api/orders/{orderId}/status

Update order status through the lifecycle.

**Request Body**:

```json
{
  "status": "accepted",
  "shipmentId": "shp-001",
  "trackingNumber": "SM-20240315-001",
  "courierProvider": "SmartCom"
}
```

**Response** `200 OK`:

```json
{
  "orderId": "ord-a1b2c3d4-e5f6-7890",
  "shipmentId": "shp-001",
  "previousStatus": "pending",
  "newStatus": "accepted",
  "trackingNumber": "SM-20240315-001",
  "updatedAt": "2024-03-15T18:00:00Z"
}
```

**Status Transitions**:

```
pending --> accepted --> ready_to_ship --> in_transit --> delivered
    |                                                       |
    +--> cancelled                                   returned
    |
    +--> rejected_shipping
```

---

## Pricing Engine

### POST /api/pricing/calculate

Calculate the complete price breakdown for a product without persisting it. Useful for price preview during product creation.

**Request Body**:

```json
{
  "vendorBasePrice": 500.00,
  "categoryId": "home-decor-textiles",
  "isVendorVatRegistered": false,
  "isNonLegalizedVendor": true,
  "parentUpliftType": "fixed",
  "parentUpliftValue": 30.00,
  "customUpliftOverride": null
}
```

**Response** `200 OK`:

```json
{
  "finalPrice": 754.28,
  "vendorBasePrice": 500.00,
  "cooperativeFee": 25.00,
  "parentVendorUplift": 30.00,
  "marketplaceUplift": 100.00,
  "logisticsSurcharge": 10.00,
  "vendorVat": 0.00,
  "aroobaVat": 18.90,
  "bucketA_vendorRevenue": 530.00,
  "bucketB_vendorVat": 0.00,
  "bucketC_aroobaRevenue": 135.00,
  "bucketD_aroobaVat": 18.90,
  "aroobaGrossMargin": 135.00,
  "aroobaMarginPercent": 19.75
}
```

**Detailed Calculation Walkthrough**:

```
Step 1: Cooperative Fee (non-legalized vendor)
  500.00 x 0.05 = 25.00 EGP

Step 2: Price After Cooperative
  500.00 + 25.00 = 525.00 EGP

Step 3: Parent Vendor Uplift (fixed 30 EGP)
  = 30.00 EGP

Step 4: Marketplace Uplift (category default 20% of price-after-coop)
  525.00 x 0.20 = 105.00 EGP
  Check minimum: 105.00 >= 15.00 (OK)
  Final marketplace uplift = 105.00 EGP
  (Note: if this were a 50 EGP item, 50 x 0.20 = 10 < 15, so 15 EGP minimum applies)

Step 5: Logistics Surcharge
  = 10.00 EGP (fixed SmartCom buffer)

Step 6: Bucket A (Vendor Revenue)
  = vendorBasePrice + parentVendorUplift
  = 500.00 + 30.00 = 530.00 EGP

Step 7: Bucket B (Vendor VAT)
  = 0.00 EGP (vendor is NOT VAT registered)

Step 8: Bucket C (Arooba Revenue)
  = cooperativeFee + marketplaceUplift + logisticsSurcharge
  = 25.00 + 105.00 + 10.00 = 140.00 EGP

Step 9: Bucket D (Arooba VAT — always applies)
  = 140.00 x 0.14 = 19.60 EGP

Step 10: Final Price (excluding Bucket E delivery)
  = 530.00 + 0.00 + 140.00 + 19.60 = 689.60 EGP

Arooba Gross Margin: 140.00 EGP (20.30% of final price)
```

---

### POST /api/pricing/check-deviation

Check whether a product price deviates significantly from the category average.

**Request Body**:

```json
{
  "productPrice": 1200.00,
  "categoryAvgPrice": 800.00,
  "threshold": 0.20
}
```

**Response** `200 OK`:

```json
{
  "isFlagged": true,
  "deviation": 50.00,
  "direction": "above",
  "message": "Product price is 50.00% above category average. This product will be flagged for manual review."
}
```

---

## Finance

### GET /api/finance/wallets/{vendorId}

Get the current wallet balance for a vendor.

**Response** `200 OK`:

```json
{
  "vendorId": "v1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "vendorName": "Nile Crafts",
  "totalBalance": 45200.00,
  "pendingBalance": 12800.00,
  "availableBalance": 32400.00,
  "lifetimeEarnings": 285000.00,
  "currency": "EGP",
  "nextPayoutDate": "2024-03-18T00:00:00Z",
  "meetsPayoutThreshold": true,
  "escrowEntries": [
    {
      "orderId": "ord-recent-001",
      "amount": 4400.00,
      "deliveryDate": "2024-03-10T14:00:00Z",
      "releaseDate": "2024-03-24T14:00:00Z",
      "daysRemaining": 9,
      "isReleasable": false
    },
    {
      "orderId": "ord-recent-002",
      "amount": 8400.00,
      "deliveryDate": "2024-03-05T10:00:00Z",
      "releaseDate": "2024-03-19T10:00:00Z",
      "daysRemaining": 4,
      "isReleasable": false
    }
  ]
}
```

---

### GET /api/finance/transactions/{orderId}

Get the 5-bucket transaction split for an order.

**Response** `200 OK`:

```json
{
  "orderId": "ord-a1b2c3d4-e5f6-7890",
  "totalAmount": 1809.00,
  "splits": [
    {
      "id": "ts-001",
      "orderItemId": "oi-001",
      "productTitle": "Hand-Painted Ceramic Vase (x2)",
      "vendorName": "Nile Crafts",
      "bucketA": 880.00,
      "bucketB": 123.20,
      "bucketC": 220.00,
      "bucketD": 30.80,
      "bucketE": 55.00,
      "totalAmount": 1309.00
    },
    {
      "id": "ts-002",
      "orderItemId": "oi-002",
      "productTitle": "Silver Ankh Necklace (x1)",
      "vendorName": "Cairo Silver",
      "bucketA": 350.00,
      "bucketB": 49.00,
      "bucketC": 52.50,
      "bucketD": 8.50,
      "bucketE": 40.00,
      "totalAmount": 500.00
    }
  ],
  "aggregated": {
    "totalBucketA": 1230.00,
    "totalBucketB": 172.20,
    "totalBucketC": 272.50,
    "totalBucketD": 39.30,
    "totalBucketE": 95.00,
    "grandTotal": 1809.00
  },
  "createdAt": "2024-03-15T16:00:00Z"
}
```

---

## Shipping

### POST /api/shipping/calculate

Calculate the shipping fee for a shipment.

**Request Body**:

```json
{
  "actualWeightKg": 1.5,
  "dimensionL": 20,
  "dimensionW": 20,
  "dimensionH": 35,
  "fromZoneId": "cairo",
  "toZoneId": "alexandria",
  "baseRate": 55.00,
  "perKgRate": 10.00
}
```

**Response** `200 OK`:

```json
{
  "actualWeight": 1.5,
  "volumetricWeight": 2.80,
  "chargeableWeight": 2.80,
  "baseFee": 55.00,
  "excessWeightFee": 18.00,
  "totalFee": 73.00,
  "subsidizedCustomerFee": 63.00,
  "aroobaSubsidy": 10.00,
  "calculation": {
    "volumetricFormula": "(20 x 20 x 35) / 5000 = 2.80 kg",
    "chargeableWeightRule": "MAX(1.5, 2.80) = 2.80 kg",
    "excessWeightCalc": "(2.80 - 1.00) x 10.00 = 18.00 EGP",
    "totalFeeCalc": "55.00 + 18.00 = 73.00 EGP",
    "subsidyNote": "SmartCom buffer of 10.00 EGP reduces customer-facing fee"
  }
}
```

---

### GET /api/shipping/zones

List all delivery zones.

**Response** `200 OK`:

```json
{
  "zones": [
    {
      "id": "cairo",
      "name": "Greater Cairo",
      "nameAr": "القاهرة الكبرى",
      "citiesCovered": ["Cairo", "Giza", "6th of October", "New Cairo", "Helwan"],
      "slaDays": "1-2"
    },
    {
      "id": "alexandria",
      "name": "Alexandria",
      "nameAr": "الإسكندرية",
      "citiesCovered": ["Alexandria", "Borg El Arab"],
      "slaDays": "1-2"
    },
    {
      "id": "delta",
      "name": "Delta",
      "nameAr": "الدلتا",
      "citiesCovered": ["Tanta", "Mansoura", "Zagazig", "Damanhur"],
      "slaDays": "2-3"
    },
    {
      "id": "upper-egypt",
      "name": "Upper Egypt",
      "nameAr": "صعيد مصر",
      "citiesCovered": ["Aswan", "Luxor", "Assiut", "Minya", "Sohag"],
      "slaDays": "3-5"
    },
    {
      "id": "canal",
      "name": "Canal Cities",
      "nameAr": "القنال",
      "citiesCovered": ["Suez", "Ismailia", "Port Said"],
      "slaDays": "2-3"
    },
    {
      "id": "sinai",
      "name": "Sinai",
      "nameAr": "سيناء",
      "citiesCovered": ["Sharm El Sheikh", "El Arish", "Dahab"],
      "slaDays": "5-7"
    }
  ]
}
```

---

## Customers

### GET /api/customers/{id}

Get a customer profile with loyalty info, addresses, and order history.

**Response** `200 OK`:

```json
{
  "id": "c-a1b2c3d4-e5f6-7890",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "fullName": "Ahmed Hassan",
  "loyaltyPoints": 1250,
  "walletBalance": 50.00,
  "referralCode": "AHMED-XK7P",
  "totalOrders": 8,
  "totalSpent": 4850.00,
  "addresses": [
    {
      "id": "addr-001",
      "label": "Home",
      "fullAddress": "42 Tahrir Square, Downtown",
      "city": "Cairo",
      "zoneId": "cairo",
      "isDefault": true
    },
    {
      "id": "addr-002",
      "label": "Office",
      "fullAddress": "15 Smart Village, Giza",
      "city": "Giza",
      "zoneId": "cairo",
      "isDefault": false
    }
  ],
  "activeSubscriptions": [
    {
      "id": "sub-001",
      "name": "Weekly Essentials Box",
      "frequency": "weekly",
      "items": [
        { "productId": "p-food-001", "quantity": 2 },
        { "productId": "p-food-002", "quantity": 1 }
      ],
      "nextDeliveryDate": "2024-03-18T10:00:00Z",
      "isActive": true
    }
  ]
}
```

---

## Dashboard

### GET /api/dashboard/stats

Get platform-wide KPIs. Requires admin role.

**Response** `200 OK`:

```json
{
  "totalGmv": 2450000.00,
  "totalOrders": 4280,
  "activeVendors": 87,
  "registeredCustomers": 12500,
  "avgOrderValue": 572.43,
  "codRatio": 0.62,
  "returnRate": 0.08,
  "monthlyGrowth": 0.15,
  "kpiStatus": {
    "codRatio": { "value": 0.62, "target": 0.65, "status": "healthy" },
    "returnRate": { "value": 0.08, "target": 0.12, "status": "healthy" },
    "orderAcceptanceRate": { "value": 0.96, "target": 0.95, "status": "healthy" },
    "firstAttemptDelivery": { "value": 0.87, "target": 0.85, "status": "healthy" },
    "activeVendorPercent": { "value": 0.73, "target": 0.70, "status": "healthy" },
    "systemUptime": { "value": 0.999, "target": 0.999, "status": "healthy" }
  },
  "period": "2024-03-01T00:00:00Z/2024-03-15T23:59:59Z"
}
```

---

## Categories

### GET /api/categories

List all product categories with uplift configuration.

**Response** `200 OK`:

```json
{
  "categories": [
    {
      "id": "jewelry-accessories",
      "nameEn": "Jewelry & Accessories",
      "nameAr": "مجوهرات وإكسسوارات",
      "uplift": { "min": 0.15, "max": 0.18, "default": 0.15 },
      "riskLevel": "low",
      "productCount": 342
    },
    {
      "id": "fashion-apparel",
      "nameEn": "Fashion & Apparel",
      "nameAr": "أزياء وملابس",
      "uplift": { "min": 0.22, "max": 0.25, "default": 0.22 },
      "riskLevel": "high",
      "productCount": 518
    },
    {
      "id": "home-decor-fragile",
      "nameEn": "Home Decor (Fragile)",
      "nameAr": "ديكور (هش)",
      "uplift": { "min": 0.25, "max": 0.30, "default": 0.25 },
      "riskLevel": "high",
      "productCount": 156
    },
    {
      "id": "home-decor-textiles",
      "nameEn": "Home Decor (Textiles)",
      "nameAr": "ديكور (منسوجات)",
      "uplift": { "min": 0.20, "max": 0.20, "default": 0.20 },
      "riskLevel": "medium",
      "productCount": 203
    },
    {
      "id": "leather-goods",
      "nameEn": "Leather Goods",
      "nameAr": "منتجات جلدية",
      "uplift": { "min": 0.20, "max": 0.20, "default": 0.20 },
      "riskLevel": "medium",
      "productCount": 89
    },
    {
      "id": "beauty-personal",
      "nameEn": "Beauty & Personal Care",
      "nameAr": "جمال وعناية شخصية",
      "uplift": { "min": 0.20, "max": 0.20, "default": 0.20 },
      "riskLevel": "medium",
      "productCount": 134
    },
    {
      "id": "furniture-woodwork",
      "nameEn": "Furniture & Woodwork",
      "nameAr": "أثاث وأعمال خشبية",
      "uplift": { "min": 0.15, "max": 0.15, "default": 0.15 },
      "riskLevel": "medium",
      "productCount": 67
    },
    {
      "id": "food-essentials",
      "nameEn": "Food & Essentials",
      "nameAr": "أغذية ومستلزمات",
      "uplift": { "min": 0.10, "max": 0.15, "default": 0.12 },
      "riskLevel": "low",
      "productCount": 245
    }
  ]
}
```

---

## Error Responses

All error responses follow a consistent format.

### 400 Bad Request (Validation Error)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "vendorBasePrice": ["Vendor base price must be greater than 0."],
    "categoryId": ["Category ID is required.", "Invalid category ID."]
  }
}
```

### 401 Unauthorized

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication token is missing or invalid."
}
```

### 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to access this resource."
}
```

### 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Vendor with ID 'v1b2c3d4-e5f6-7890-abcd-ef1234567890' was not found."
}
```

### 500 Internal Server Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "traceId": "00-abcdef1234567890-abcdef12-00"
}
```

---

## Rate Limiting

API rate limits apply per authenticated user:

| Tier | Limit | Window |
|------|-------|--------|
| Standard | 100 requests | Per minute |
| Vendor | 200 requests | Per minute |
| Admin | 500 requests | Per minute |

Rate limit headers are included in every response:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1710504060
```

---

## Pagination

All list endpoints support pagination with consistent response format:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```
