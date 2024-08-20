# What is this?

This folder contains all of the code in the original ChessOfCards API. It has not been deleted so that we can refer to it for domain knowledge. Be mindful that this code does nothing. The solution is now configured to use code in the `src` and `tests` directories at the root of the project.

---

# Changes from old API to new API

-   Moved from layered architecture to vertical slices.
-   Separated application layers into different projects to avoid cyclical dependencies.
-   Moved much of the domain logic into the domain models out of the service layer. This is to avoid an anemic domain model architecutre anti-pattern.
