# Design - duplicated-notification-codex

## Decision
Use `Dictionary<string, double?>` as cache model in `ItemSearcher`:
- key: `Item.Id`
- value: last known processed current price
- nullable value supports legacy IDs with unknown historical price.

## Rationale
- Enables explicit business rule check `current < cached`.
- Keeps cache lookup O(1) by item identity.
- Allows backward compatibility without a migration script.

## Flow
```mermaid
sequenceDiagram
    participant S as ItemSearcher
    participant W as WallapopRepository
    participant C as DistributedCache
    participant A as Alert Aggregate
    participant E as EventBus

    S->>W: Latest(alert.Url)
    S->>C: Get(cacheKey)
    Note over S: Deserialize dictionary or legacy ID list

    loop each eligible item
        alt item not cached
            Note over S: Add as normal notification
        else cached and current < cachedPrice
            Note over S: Add notification with "(Baja de Precio)"
        else cached and current >= cachedPrice
            Note over S: Skip notification
        end
        Note over S: Update cache price with current
    end

    S->>C: Set(cacheKey, id->price map)

    alt items to notify exist
        S->>A: alert.NewSearch(items)
        S->>E: Publish(domain events)
    else no items
        S->>A: RecordSearch(now)
    end
```

## Key implementation notes
- Price-drop suffix constant: `(Baja de Precio)`.
- Suffix is appended once per generated notification title.
- Cache update runs after evaluation so dropped prices are persisted immediately.
- Legacy cache `List<string>` is converted to dictionary with null price values.
