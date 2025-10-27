# Test Strategy: Hot Reload for .NET @ JetBrains Rider

### Testing Environment:
*   **Rider build:** JetBrains Rider 2025.3 EAP 7
*   **.NET SDK:** 10.0.100-rc.2.25502.107
*   **OS:** macOS Tahoe 26.1 Beta (25B5072a)
*   **Sample project:** https://github.com/jussifrewt/RiderHotReloadTest

---

## 1. Goal

My goal is to validate that Hot Reload in Rider delivers on its core promise: accelerating development without sacrificing stability. I will approach this feature from a developer's perspective, focusing on real-world scenarios, debugging workflows, and interactions with the file system. My objective is to determine if Hot Reload is a tool a developer can trust implicitly for daily work.

---

## 2. Scope

### In scope:
*   **Primary platform:** ASP.NET Core web apps on macOS.
*   **Execution modes:** Testing will cover both **Run** and **Debug** modes.
*   **Core functionality:** Validating that UI changes (Razor) and C# logic updates (including `async/await` and `LINQ`) are applied correctly.
*   **Architecture:** Verifying updates in injected dependencies (Dependency Injection).
*   **User experience:**
    *   Ensuring client-side DOM state is preserved during server-side updates.
    *   Verifying correct functionality and rendering on simulated **mobile/tablet devices**.
*   **IDE Integration & Environment:** This is a critical focus area. Testing will include:
    *   Verifying Rider's UI feedback mechanisms (`Apply Changes` banner, error popups).
    *   Validating the core **IDE settings** for enabling/disabling Hot Reload.
    *   Ensuring Rider correctly detects file changes made in **external editors**.
    *   Testing for conflicts with other core IDE features like the **Profiler**, **VCS (Git)**, and **Static Code Analysis**.

### Out of scope:
*   Performance, stress, and load testing.
*   Niche project types (e.g., Blazor WASM) and complex language features not covered by the core scenarios.
*   Exhaustive negative testing of every possible unsupported change. The focus is on the most common failure modes (build failures).

---

## 3. Key Risks and Test Scenarios

My risk analysis is structured into four distinct categories:
*  **Core Functionality (The "Happy Path"):** Does the feature work as advertised?
*  **Stability & Error Handling (Graceful Failure):** How does the feature behave when things go wrong?
*  **Integration & Environment:** How does it interact with the debugger, file system, and user's context?
*  **Advanced IDE Integration:** How does it coexist with other complex Rider features like profiling and VCS?

Each scenario is designed to answer a specific question about the feature's reliability.

### Core Functionality Scenarios (The "Happy Path")
*These tests validate that the feature works as advertised in the most common scenarios.*

| Priority     | Risk Description                                                                                                                                  | Corresponding Test Scenario                                                                                                                                           |
|:-------------|:--------------------------------------------------------------------------------------------------------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Critical** | **Basic UI & Logic Failure:** The most fundamental UI (Razor) and C# code-behind changes are not applied in Run Mode.                             | **Patches 1 & 2:** Sequentially apply basic UI and Logic changes on the Home page. **Expected:** The page content updates as described in the patches.                |
| **High**     | **Modern C# Support Failure:** The feature fails on common language constructs like LINQ or async/await.                                          | **Patches 3 & 4:** Apply LINQ query and async/await changes on the Advanced page. **Expected:** The data filtering logic and async method behaviour update correctly. |
| **High**     | **Architectural (DI) Failure:** The feature only tracks changes in the immediate file and fails to detect modifications in injected dependencies. | **Patch 5:** Modify the `GetMessage()` method in the external `MessageService.cs` file. **Expected:** The change is detected and reflected on the Advanced page.      |

### Stability & Error Handling Scenarios (Graceful Failure)
*These are the top priority after the happy path. A tool that crashes is worse than one that doesn't work at all.*

| Priority     | Risk Description                                                                                                                                                                                                                      | Corresponding Test Scenario                                                                                                                                                                                                                                                                              |
|:-------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Critical** | **API Breaking Change Crash:** Applying a change to a method's public signature (a "breaking change") causes the application or dotnet watch to crash.                                                                                | **Action:** Manually edit the signature of the GetMessage() method (e.g., add a parameter: `GetMessage(int id))`. **Expected:** The app continues running with its last good state. dotnet watch clearly reports an error `(ENC0009: Updating the type of method requires restarting the application.)`. |
| **Critical** | **Build Failure Crash:** A syntax error is introduced. The running application crashes instead of continuing to run.                                                                                                                  | Introduce a syntax error in `MessageService.cs`. **Expected:** The app continues running. dotnet watch reports a compilation error. After fixing the error, Hot Reload applies successfully.                                                                                                             |
| **High**     | **IDE Feedback Failure on Error:**  A compilation error occurs, but the Rider UI provides no clear, actionable feedback.                                                                                                              | **Action:** Execute the "API Breaking Change" and "Build Failure" scenarios from within Rider. **Expected:** Rider must display a clear notification or error popup explaining the compilation error and linking to the source of the problem.                                                           |

### Integration & Environment Scenarios
*These tests verify how Hot Reload interacts with the debugger, file system, settings and client devices.*

| Priority     | Risk Description                                                                                                                                                                     | Corresponding Test Scenario                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
|:-------------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Critical** | **Debugger Corruption:** Applying Hot Reload during a debug session corrupts the debugger state or crashes it.                                                                       | While paused on a breakpoint, apply a patch (e.g., Patch 3) and then step through the code. **Expected:** The variable inspector shows the new, correct value, and debugging can continue.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| **High**     | **IDE Settings Ignored:** The toggles in Rider's settings (e.g., "Enable Hot Reload") do not correctly control the feature, breaking the user's ability to manage their environment. | With the app running, verify Hot Reload works. Go to `Settings > Build, Execution, Deployment > Hot Reload` and **disable** the "Enable Hot Reload" when debugging. Restart the application. Make a file change. **Expected:** Hot Reload is **inactive**. Re-enable the setting, restart, and verify functionality is restored. With the app running, verify Hot Reload works. Go to `Settings > Build, Execution, Deployment > Hot Reload` and **disable** the "Enable Hot Reload when running without debugging". Restart the application. Make a file change. **Expected:** Hot Reload is **inactive**. Re-enable the setting, restart, and verify functionality is restored. |
| **High**     | **External File Changes:** Rider's file system watcher fails to detect changes made in an external editor.                                                                           | With the app running from Rider, make a change to `Index.cshtml` in an external editor (e.g., VS Code). **Expected:** Rider's Apply Changes banner appears within a few seconds.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| **High**     | **User Experience (UX) - State Loss:** The feature works but destroys the developer's working context (e.g., data entered in forms).                                                 | Perform the UI State Preservation Test on the Advanced page as described in the README.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| **Medium**   | **Client-Side Compatibility:** A Hot Reload change on the server-side breaks rendering on specific client devices.                                                                   | After applying each patch, verify that the web application continues to render and function correctly on a **simulated tablet/mobile device**.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |

### Advanced IDE Integration Scenarios
*These tests probe the interaction of Hot Reload with other core IDE features, targeting potential conflicts and race conditions.*

| Priority   | Risk Description                                                                                                                           | Corresponding Test Scenario                                                                                                                                                                                                                                                   |
|:-----------|:-------------------------------------------------------------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **High**   | **Profiling Conflict:** Applying Hot Reload while a profiling session is active causes the application, the profiler, or the IDE to crash. | Launch the application with the profiler attached. While profiling, apply a patch. **Expected:** The application and profiler **do not crash**. A graceful error is reported at minimum.                                                                                      |
| **Medium** | **VCS (Git) Conflict:** A mass file change from a Git operation is not handled correctly by the file watcher.                              | With the app running, use Rider's UI to Stash several uncommitted changes. **Expected:** Hot Reload triggers and reverts the application to the `HEAD` state. Use Rider's UI to Unstash the changes. **Expected:** Hot Reload triggers again and applies the stashed changes. |
| **Medium** | **Static Analysis Desync:** Rider's code analyzer gets out of sync with the running application after a Hot Reload.                        | Introduce a code warning and apply Hot Reload. Verify the "Problems" view shows the warning. Fix the code and apply Hot Reload. **Expected:** The warning **disappears** from the "Problems" view without a manual refresh.                                                   |
| **Low**    | **NuGet Restore Conflict:** A NuGet package update performed while the application is running leads to an inconsistent state.              | With the app running, use Rider's NuGet tool to update a package. **Expected:** dotnet watch / Rider should detect the project file change and correctly suggest a **manual restart**.                                                                                        |

---

## 4. System Analysis & Dependencies

Before creating an execution plan, I analyzed the system to understand its architecture and identify critical points of failure. Hot Reload and Rider operate in a tight client-server loop where a failure at any handoff point can break the entire feature.

### HOW HOT RELOAD AND THE APP DEPEND ON RIDER
1.  **Launch configuration (top priority):** Rider must correctly parse `launchSettings.json` to build and execute the right `dotnet` command.
    *   **Why it's critical:** This is the entry point. A failure here means the Hot Reload session **never even starts**.
2.  **File system watcher:** Rider is responsible for reliably detecting file changes on disk.
    *   **Why it's critical:** This is the **primary trigger** for Hot Reload and the most likely environment-related failure point. An unreliable watcher makes the feature unpredictable and destroys user trust.
3.  **Debugger proxy/adapter:** In Debug mode, Rider injects its debugger into the running process.
    *   **Why it's critical:** A failure in this fragile handshake will break Hot Reload specifically in debugging sessions, which is one of its most valuable use cases.

### HOW RIDER DEPENDS ON THE .NET WATCHER PROCESS
1.  **Feedback protocol (top priority):** `dotnet watch` reports its state ("changes applied," "errors," etc.). Rider's UI is entirely dependent on correctly parsing this output.
    *   **Why it's critical:** This is the core of the user experience. If the UI lies or is silent about an error, the developer is left confused and frustrated, leading them to abandon the feature.
2.  **Process lifecycle:** Rider must correctly start, stop, and manage the lifecycle of the `dotnet watch` process.
    *   **Why it's critical:** A failure here can lead to "zombie" processes consuming system resources or requiring a manual restart of the IDE, disrupting the development flow.

---

## 5. Work Organization & Execution Strategy

Based on the **Key Risks** (Section 3) and **System Analysis** (Section 4), I have structured the testing effort into a phased plan.

### GUIDING PRINCIPLES
*   **Risk-First:** I will prioritize work strictly according to the risk matrix. The highest-impact failures must be discovered first.
*   **Build Confidence in Layers:** I will start with the core technology (the .NET SDK) in isolation. Once the foundation is proven stable, I will incrementally add the complexity of the Rider integration.
*   **Prioritize Blocker Discovery (Fail Fast):** I will take the "must-have negative scenarios" early. Finding a blocking issue like a crash on day one is a major success, as it saves the most time for the development team.

### PHASED EXECUTION

**Phase 1: Foundation and stability baseline**
*Verify the core .NET SDK Hot Reload mechanism is stable before involving the IDE.*
*   **Actions:**
1.  Execute all **Stability & Error Handling Scenarios** ("API Changes", "Build Failure") using only the CLI (`dotnet watch`).
2.  If stable, run all **Core Functionality** patch tests (Patches 1–5).

**Phase 2: Rider Integration & Environment**
*Test Rider's primary integration points and environmental interactions.*
*   **Actions:**
1.  Launch from Rider in **Run Mode**.
2.  Execute scenarios from the **"Integration & Environment Scenarios"** table, including the critical **IDE Settings** test.

**Phase 3: Rider Advanced Integration & Debugger**
*Exercise the most complex integration points: the debugger and other advanced IDE features.*
*   **Actions:**
1.  Launch from Rider in **Debug Mode** and systematically run all **"Debugger Integration Risks"** scenarios.
2.  Execute all scenarios from the **"Advanced IDE Integration Scenarios"** table (Profiling, VCS, etc.).

**Phase 4: Exploratory Testing and Documentation**
*Find "unknown unknowns" and finalize deliverables.*
*   **Actions:**
1.  Conduct targeted exploratory sessions based on the most complex areas (e.g., rapid changes during a debug session).
2.  Review all bug reports and finalize the Test Summary Report.

### ORGANIZATION AND DELIVERABLES

This section outlines the final artifacts that would be produced at the end of the testing effort. While not all of these are created for this assignment (e.g., actual bug reports), they represent the complete set of deliverables for a real-world task.

*   **Tooling:**
    *   **Version control:** Git
    *   **IDE:** JetBrains Rider (Stable + EAP)
    *   **Execution:** Terminal (`dotnet watch`) & Rider's runner
    *   **Bug tracking:** YouTrack (or similar)
*   **Deliverables:**
    *  **GitHub sample project:** The reproducible testbed with patches.
    *  **This Test Strategy Document:** The comprehensive plan guiding the work.
    *  **A set of high-quality Bug Reports:** For any discovered issue, a clear report would be created.
    *  **Final Test Summary Report (TSR):** A concise summary of work done, quality assessment, and key findings.
    
---

## 6. Automation vs. Manual Strategy

### WHERE MANUAL/EXPLORATORY TESTING IS ESSENTIAL
*   **Debugger scenarios:** Evaluating live debug state needs human judgment.
*   **Rider UI/UX feedback:** Timing and clarity of banners/notifications require eyes-on testing.
*   **Environment/tooling:** External editors and Git flows are too variable for reliable automation.
*   **Negative paths:** Graceful failure and error messaging need human interpretation.

### WHERE AUTOMATION ADDS REAL VALUE
*   **Core SDK Smoke Test:** A small, non-visual Web API test is the ideal candidate. Its purpose is to act as a **regression guard** for the core Hot Reload engine, completely independent of the Rider UI.
    *   **CI/CD Integration:** This test might be added to the CI/CD pipeline and parameterized to run on all target operating systems (e.g., Windows Server, macOS) for every new build of the .NET SDK. This will catch major cross-platform regressions early and automatically.
