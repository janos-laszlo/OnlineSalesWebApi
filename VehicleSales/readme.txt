To use this project:
-Add the following to the configuration:
    -'ConnectionStrings:MariaDB': a connection string to MariaDB
    -"R2": {
        "AccountId": "",
        "AccessKeyId": "",
        "SecretAccessKey": ""
      }
-call AddVehicleSales on the service collection
-it depends on the UserIdentity.User entity/table existing. 

-Photo upload sequence diagram:
Client                          API                         Storage (S3/Azure)
  │                              │                               │
  │── POST /vehicle-sales ──────>│                               │
  │                              │── generate object keys        │
  │                              │── create entity (Draft)       │
  │                              │── generate presigned URLs ───>│
  │<─ 201 { id, presignedUrls } ─│                               │
  │                              │                               │
  │── PUT {presignedUrl[0]} ────────────────────────────────────>│
  │── PUT {presignedUrl[1]} ────────────────────────────────────>│
  │                              │                               │
  │── POST /vehicle-sales/{id}/photos/confirm ──────────────────>│
  │                              │── verify objects exist ──────>│
  │                              │── set entity Active           │
  │<─ 204 No Content ────────────│                               │


  -background job that runs every 5 minutes to check for Draft entities that are older than 1 hour and deletes them
  along with any uploaded photos.This ensures that incomplete listings don't clutter
  the system and that storage isn't wasted on unused photos.
