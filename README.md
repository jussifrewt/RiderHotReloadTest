# Hot Reload Demonstration Project

This project is designed to demonstrate a test approach for the Hot Reload feature in Rider using Git patches. It shows how UI and C# code logic changes can be applied to a running application without a restart.

### Setup

1.  Clone this repository.
2.  Open a terminal in the root folder.
3.  Run `dotnet restore` to install dependencies.

### How to Demonstrate

This demonstration shows how to apply UI and logic changes to a running application without restarting it.

**Part 1: Initial State**

1.  Start the application by running the command:
    ```bash
    dotnet run
    ```
2.  Open the application in your browser (e.g., at `http://localhost:5001`).
3.  Observe the initial state:
    *   The header says: **"Welcome"**.
    *   The message below says: **"This is the initial logic."**.
4.  **Keep the application running for the next steps.**

**Part 2: Demonstrating UI Hot Reload**

1.  Open a **new terminal window** in the same project folder.
2.  Apply the UI patch using Git:
    ```bash
    git apply 0001-feat-Demonstrate-UI-Hot-Reload.patch
    ```
3.  Rider will detect the file change and trigger Hot Reload.
4.  **Refresh the page in your browser.**
5.  Observe the result:
    *   The header now says: **"UI Hot Reload is working!"**.
    *   The logic message remains unchanged.

**Part 3: Demonstrating Logic Hot Reload**

1.  In the same terminal, apply the logic patch:
    ```bash
    git apply 0002-feat-Demonstrate-Logic-Hot-Reload.patch
    ```
2.  Rider will detect the change and trigger Hot Reload.
3.  **Refresh the page in your browser.**
4.  Observe the final result:
    *   The message now says: **"The code logic has been UPDATED by Hot Reload!"**.

### Cleanup

To revert the project to its initial state after applying the patches, run the following command:
```bash
git reset --hard
```