> **Note:** The complete Test Strategy can be found in the [`Test-Strategy.md`](Test-Strategy.md) file. This document (`README.md`) only contains the practical instructions for running the demonstration patches.

---
# Hot Reload Demonstration Project

This project is a testbed designed to demonstrate and validate the Hot Reload feature in Rider. It uses a series of Git patches to simulate a developer making various types of changes to a running application, from simple UI tweaks to modifications in dependent services.

### Setup

1.  Clone this repository.
2.  Open a terminal in the root folder.
3.  Run `dotnet restore` to install dependencies.

### How to Demonstrate

**Part 1: Initial State**

1.  Start the application in watch mode, which is required for Hot Reload:
    ```bash
    dotnet watch
    ```
2.  Open the application in your browser. Observe the initial state on both the **Home** page and the **Advanced Tests** page.
3.  **Keep the application running for all subsequent steps.**

**Part 2: Test Scenarios Demonstration**

Open a **new terminal window** in the same project folder. Apply the following patches one by one, refreshing the relevant browser page after each step to observe the changes.

---

#### **Basic Scenarios (Home Page)**

**Test 1: Basic UI Hot Reload**
*   **Action:** Apply the first patch.
    ```bash
    git apply 0001-test-Demonstrate-basic-UI-Hot-Reload.patch
    ```
*   **Expected Result:** On the **Home** page, the header "Welcome!" changes to "UI Hot Reload is working!".

**Test 2: Basic Logic Hot Reload**
*   **Action:** Apply the second patch.
    ```bash
    git apply 0002-test-Demonstrate-basic-Logic-Hot-Reload.patch
    ```
*   **Expected Result:** On the **Home** page, the message "This is the initial logic." changes to "The code logic has been UPDATED by Hot Reload!".

---

#### **Advanced Scenarios (Advanced Tests Page)**

**Test 3: LINQ Query Hot Reload**
*   **Action:** Apply the LINQ test patch.
    ```bash
    git apply 0003-test-Demonstrate-LINQ-Hot-Reload.patch
    ```
*   **Expected Result:** On the **Advanced Tests** page, the list of countries will be filtered by the letter 'B', showing only "Belgium".

**Test 4: Async/Await Hot Reload**
*   **Action:** Apply the async test patch.
    ```bash
    git apply 0004-test-Demonstrate-async-await-Hot-Reload.patch
    ```
*   **Expected Result:** On the **Advanced Tests** page, the page will take approx. 1 second to load, and the async message will change.

**Test 5: Dependency Injection Hot Reload**
*   **Action:** Apply the DI test patch.
    ```bash
    git apply 0005-test-Demonstrate-DI-Hot-Reload.patch
    ```
*   **Expected Result:** On the **Advanced Tests** page, the message from the service will be updated, proving that changes in dependencies are tracked.

---

#### **Special Test: UI State Preservation**

1.  Navigate to the **Advanced Tests** page.
2.  Enter any text into the input field under "UI State Preservation Test".
3.  Apply **any** of the patches above.
4.  **Observe the browser without refreshing:** The text you entered should remain in the input field. This proves that Hot Reload itself does not cause a full page reload, preserving the client-side DOM state.

### Cleanup

To revert the project to its initial state after applying the patches, run:
```bash
git reset --hard
```
---

### Negative Scenarios (Graceful Failure)

These manual tests are designed to verify that the application and tooling remain stable and provide clear feedback when errors occur.

**Test 1: "API Change" Handling**
1.  With the application running, open the `MessageService.cs` file.
2.  Modify the signature of the `GetMessage()` method (e.g., add a parameter: `public string GetMessage(int id)`).
3. **Observe the `dotnet watch` terminal:**
    *   **Expected:** The terminal should report a problem and a restart is required. The application itself **should not crash**.
    *   `watch : error ENC0009: Updating the type of method requires restarting the application.`
4. **Observe the Rider app:**
    *   **Expected:** The Rider should report "MessageService.cs(4, 23): [ENC0009]" and a restart is required. The application itself **should not crash**.
    *   Hot Reload is not applied.

**Test 2: Build Failure Handling**
1.  Revert the "API Change" from the previous step.
2.  Now, introduce a syntax error in the same file (e.g., delete the semicolon at the end of the `return` statement).
3. **Observe the `dotnet watch` terminal:**
    *   **Expected:** The terminal should report a compilation error. The application itself **should not crash**.
    *   `... error CS1002: ; expected`
4. **Observe the Rider app:**
    *   **Expected:** The Rider should report a compilation error. The application itself **should not crash**.
    *   `MessageService.cs(11, 61): [CS1002] ; expected`
5. Fix the syntax error.
6. **Expected:** Hot Reload should now apply the changes successfully.
