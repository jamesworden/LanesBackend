# ClassroomGroups Developer Commentary

## Data Access Model Conventions

### Key Vs. Id
Private, internal primary keys of tables end with `Key` while public facing identifiers for rows of data end with `Id`. In other words, anything that is a Key is database specific and to be used internally. Alternatively, users can see Id's without an issue.

## Domain Model Conventions

### View Models
Often, our domain models will have properties that we don't want to expose to our API endpoints. An example of this is `Account.Key`. The only identification property that clients should have access to is `Account.Id`. For this reason, we must transform an `Account` to an `AccountView`, where every property is the same except the key property no longer exists on the view model. A domain model with "View" appended to the end of it hides properties from the initial model that the user should not see.