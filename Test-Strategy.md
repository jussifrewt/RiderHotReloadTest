# Test Strategy: Hot Reload for .NET @ JetBrains Rider

### Testing Environment:
*   **Rider build:** JetBrains Rider 2025.3 EAP 7
*   **.NET SDK:** 10.0.100-rc.2.25502.107
*   **OS:** MacOS Tahoe 26.1 Beta (25B5072a)
*   **Sample project:** https://github.com/jussifrewt/RiderHotReloadTest

---

## 1. Goal

My goal is to validate that Hot Reload in Rider delivers on its core promise: accelerating development without sacrificing stability. I will approach this feature from a developer's perspective, focusing on real-world scenarios, debugging workflows, and interactions with the file system. My objective is to determine if Hot Reload is a tool a developer can trust implicitly for daily work.

---

## 2. Scope

### In scope:
*   **Primary platform:** ASP.NET Core web apps on MacOS.
*   **Execution modes:** I'll test both Run and Debug modes.
*   **Core functionality:** Checking that UI changes (Razor) and C# logic updates (including async/await and LINQ) are applied correctly.
*   **Architecture:** Verifying updates in injected dependencies (Dependency Injection).
*   **User experience:**
*   *   Making sure client-side DOM state is preserved.
    *   Verifying correct functionality and rendering on simulated mobile/tablet devices.
*   **IDE integration:**
    *   Verifying Rider's UI feedback mechanisms (Apply Changes banner, error popups).
    *   Validating the core IDE settings for enabling/disabling Hot Reload.
    *   Ensuring Rider correctly detects file changes made in external editors.
    *   Testing for conflicts with other core IDE features like the Profiler, VCS (Git), and Static Code Analysis.

### Out of scope:
*   Performance, stress, and load testing.
*   Niche project types (e.g., Blazor WASM) and complex language features not covered by the core scenarios.
*   Exhaustive negative testing of every possible unsupported change. The focus is on the most common failure modes ("Rude Edits", build failures).

---

## 3. Key Risks and Test Scenarios
My risk analysis is structured into four distinct categories:
*   **Core Functionality (The "Happy Path"):** Does the feature work as advertised?
*   **Stability & Error Handling (Graceful Failure):** How does the feature behave when things go wrong?
*   **Integration & Environment:** How does it interact with the debugger, file system, and user's context?
*   **Advanced IDE Integration:** How does it coexist with other complex Rider features like profiling and VCS?

Each scenario is designed to answer a specific question about the feature's reliability.

### Core Functionality Scenarios (The "Happy Path")
*These tests validate that the feature works as advertised in the most common scenarios.*

| Priority | Risk Description | Corresponding Test Scenario |
|:---|:---|:---|
| **Critical** | **Basic UI & Logic Failure:** The most fundamental UI (Razor) and C# code-behind changes are not applied in Run Mode. | **Patches 1 & 2:** Sequentially apply basic UI and Logic changes on the Home page. **Expected:** The page content updates as described in the patches. |
| **High** | **Modern C# Support Failure:** The feature fails on common language constructs like LINQ or async/await. | **Patches 3 & 4:** Apply LINQ query and async/await changes on the Advanced page. **Expected:** The data filtering logic and async method behaviour update correctly. |
| **High** | **Architectural (DI) Failure:** The feature only tracks changes in the immediate file and fails to detect modifications in injected dependencies. | **Patch 5:** Modify the `GetMessage()` method in the external `MessageService.cs` file. **Expected:** The change is detected and reflected on the Advanced page. |

### Stability & Error Handling Scenarios (Graceful Failure)
*These are the top priority after the happy path. A tool that crashes is worse than one that doesn't work at all.*

| Priority | Risk Description | Corresponding Test Scenario |
|:---|:---|:---|
| **Critical** | **"Rude Edit" Crash:** An unsupported change (e.g., modifying a method signature) causes the application or the dotnet watch process to crash. | Manually edit the signature of the GetMessage() method. **Expected:** The app continues running. dotnet watch / Rider clearly reports a "rude edit" and suggests a manual restart. |
| **Critical** | **Build Failure Crash:** A syntax error is introduced. The running application crashes instead of continuing to run. | Introduce a syntax error in MessageService.cs. **Expected:** The app continues running. dotnet watch / Rider reports a compilation error. After fixing the error, Hot Reload applies successfully. |
| **High** | **IDE Feedback Failure on Error:** An error (like a "Rude Edit") occurs, but the Rider UI provides no clear, actionable feedback. | Execute the "Rude Edit" and "Build Failure" scenarios from within Rider. **Expected:** Rider must display a clear notification or error popup explaining the problem. |

### Integration & Environment Scenarios
*These tests verify how Hot Reload interacts with the debugger, file system, settings and client devices.*

| Priority | Risk Description | Corresponding Test Scenario                                                                                                                                                                |
|:---|:---|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Critical** | **Debugger Corruption:** Applying Hot Reload during a debug session corrupts the debugger state or crashes it. | While paused on a breakpoint, apply a patch (e.g., Patch 3) and then step through the code. **Expected:** The variable inspector shows the new, correct value, and debugging can continue. |
| **High** | **External File Changes:** Rider's file system watcher fails to detect changes made in an external editor. | With the app running from Rider, make a change to Index.cshtml in an external editor (e.g., VS Code). **Expected:** Rider's Apply Changes banner appears within a few seconds.             |
| **High** | **User Experience (UX) - State Loss:** The feature works but destroys the developer's working context (e.g., data entered in forms). | Perform the UI State Preservation Test on the Advanced page as described in the README.                                                                                                    |
| **Medium** | **Client-Side Compatibility:** A Hot Reload change on the server-side breaks rendering on specific client devices, especially mobile/tablet browsers. | After applying each patch, verify that the web application continues to render and function correctly on a simulated tablet/mobile device<br/>.                                                 |

### Advanced IDE Integration Scenarios
*These tests probe the interaction of Hot Reload with other core IDE features, targeting potential conflicts and race conditions.*

| Priority   | Risk Description                                                                                                                                                                            | Corresponding Test Scenario                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
|:-----------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **High**   | **IDE Settings Ignored or Cause Instability:** The toggles in Rider's settings (e.g., "Enable Hot Reload", "Hot Reload on Save") do not correctly control the feature or cause instability. | With the app running, verify Hot Reload works (banner appears). Go to Settings > Build, Execution, Deployment > Hot Reload and disable the "Enable Hot Reload" checkbox. Restart the application. Make a file change. **Expected:** The "Apply Changes" banner does not appear and Hot Reload is inactive. Re-enable the setting, restart the application, and verify that functionality is restored.                                                                                                                                                                                  |
| **High**   | **Profiling Conflict:** Applying Hot Reload while a profiling session is active causes the application, the profiler, or the IDE to crash.                                                  | Launch the application with the profiler attached (Profile 'RiderHotReloadTest: http' with Sampling). While profiling, apply a patch. **Expected:** The application and profiler do not crash. A graceful error is reported at minimum.                                                                                                                                                                                                                                                                                                                                                |
| **Medium** | **VCS (Git) Conflict:** A mass file change from a Git operation is not handled correctly by the file watcher.                                                                               | With the app running, use Rider's UI to Stash several uncommitted changes. **Expected:** Hot Reload triggers and reverts the application to the HEAD state. Use Rider's UI to Unstash the changes. **Expected:** Hot Reload triggers again and applies the stashed changes.                                                                                                                                                                                                                                                                                                            |
| **Medium** | **Static Analysis Desync:** Rider's code analyzer gets out of sync with the running application after a Hot Reload.                                                                         | Introduce a code warning and apply Hot Reload. Verify the "Problems" view shows the warning. Fix the code and apply Hot Reload. **Expected:** The warning disappears from the "Problems" view without a manual refresh.                                                                                                                                                                                                                                                                                                                                                                |
| **Low**    | **NuGet Restore Conflict:** A NuGet package update performed while the application is running leads to an inconsistent state.                                                               | With the app running, use Rider's NuGet tool to update a package. **Expected:** dotnet watch / Rider should detect the project file change and correctly suggest a manual restart.                                                                                                                                                                                                                                                                                                                                                                                                     |
---

## 4. System Analysis & Dependencies
Before creating an execution plan, it's crucial to analyze the system to understand its architecture and identify critical points of failure. Hot Reload and Rider operate in a tight client-server loop. A failure at any handoff point can break the entire feature. This analysis identifies the most critical dependencies.

### HOW HOT RELOAD AND THE APP DEPEND ON RIDER
1.  **Launch configuration (top priority):** Rider has to read `launchSettings.json` and build the correct `dotnet` command.
    *   **Why it's critical:** This is the entry point. If it fails, Hot Reload never even starts.
2.  **File system watcher:** Rider needs to reliably detect file changes on disk.
    *   **Why it's critical:** This is the main trigger for Hot Reload and the most likely environment-related failure point.
3.  **Debugger proxy/adapter:** In Debug, Rider injects its debugger. If that handshake breaks, Hot Reload fails specifically in debug sessions.

### HOW RIDER DEPENDS ON THE .NET WATCHER PROCESS
1.  **Feedback protocol (top priority):** `dotnet watch` reports its state ("changes applied," "rude edit," etc.). Rider's UI depends on parsing this output correctly.
    *   **Why it's critical:** This is the core of the user experience. If the UI lies, the user loses trust in the tool.
2.  **Process lifecycle:** Rider must correctly start, stop, and restart the `dotnet watch` process.

---

## 5. Work Organization & Execution Strategy
Based on the **System Analysis** (Section 4) and the **Key Risks** (Section 3), I have structured the testing effort into a phased plan governed by a set of core principles.

### GUIDING PRINCIPLES
*  **Risk-First:** I will prioritize work strictly according to the risk matrix (Section 3). The highest-impact failures must be discovered first.
*  **Build Confidence in Layers:** I will start with the core technology (the .NET SDK) in isolation. Once the foundation is proven stable, I will incrementally add the complexity of the Rider integration and the debugger.
  *  **Fail Fast:** I will take the "must-have negative scenarios" early. Finding a blocking issue like a crash on day one is a major success, as it saves time for the development team.

### PHASED EXECUTION

**Phase 1: Foundation and stability baseline**

*Verify that the core .NET SDK Hot Reload mechanism is stable and functional before involving the IDE.*
*   **Actions:**
1. Execute all Stability & Error Handling Scenarios ("Rude Edit", "Build Failure") using only the CLI (dotnet watch).
2. If stable, run all Core Functionality patch tests (Patches 1–5).

**Phase 2: Rider integration and environment**

*Test Rider's primary integration points and environmental interactions.*
*   **Actions:**
1. Launch from Rider in Run mode.
2. Repeat a subset of patch tests to validate the Apply Changes banner.
3. Execute all scenarios from the "Integration & Environment Scenarios" table, including:
   *   External File Changes test.
   *   UI State Preservation test.
   *   Client-Side Compatibility check.
4. Execute the IDE Settings test (enabling/disabling Hot Reload).

**Phase 3: Rider Advanced Integration & Debugger**

*Exercise the most complex integration points: the debugger and other advanced IDE features.*
*   **Actions:**
1. Launch from Rider in Debug Mode and systematically run all "Debugger Integration Risks" scenarios.
2. Execute all scenarios from the "Advanced IDE Integration Scenarios" table:
   *   Profiling Conflict test.
   *   VCS (Git) Conflict test.
   *   Static Analysis Desync test.
   *   NuGet Restore Conflict test.
3. Watch the debugger state and overall IDE stability closely.

**Phase 4: Exploratory testing and Documentation**

*Find "unknown unknowns”.*
*   **Actions:**
1. Conduct targeted exploratory sessions based on the most complex areas (e.g., rapid changes during a debug or profile session).

### ORGANIZATION AND DELIVERABLES
This section outlines the final artifacts that would be produced at the end of testing effort. While not all of these are created for this specific assignment (e.g., actual bug reports), they represent the complete set of deliverables for a real-world testing task.

*   **Tools**
1. **Version control:** Git for managing the sample project and patches.
2. **IDE:** JetBrains Rider (Stable + EAP for side-by-side comparison).
3. **Execution:** Terminal (dotnet watch) for the baseline; Rider's runner for integration/debug.
      
*   **Artefacts and deliverables:**
1. **GitHub sample project:** Reproducible testbed with patches and scripts.
2. **This Test Strategy Document:** The plan guiding testing process.
3. **A set of high-quality Bug Reports:** For any discovered issue, a clear report would be created in a tool like YouTrack, including precise steps to reproduce, logs, and environment details.
4. **Final Test Summary Report:** Summary page with overall quality, key findings, and remaining risks.

---

## 6. Automation vs. Manual Strategy

### WHERE MANUAL/EXPLORATORY TESTING IS ESSENTIAL
*   **Debugger scenarios:** Evaluating live debug state (locals, call stack, stepping) needs human judgment.
*   **Rider UI/UX feedback:** Timing, clarity, and correctness of banners/notifications/logs require eyes-on testing.
*   **Environment/tooling:** External editors and Git flows are too variable for reliable automation.
*   **Negative paths:** Graceful failure and error messaging need human interpretation.

### WHERE AUTOMATION ADDS REAL VALUE
*   **Core SDK Smoke Test:** A small, non-visual Web API test that runs in CI. Its purpose is to act as a regression guard for the core Hot Reload engine, completely independent of the Rider UI.
This test might be added to the CI/CD pipeline and parameterized to run on all target operating systems (e.g., Windows Server, macOS, etc) for every new build of the .NET SDK. This will catch major cross-platform regressions early.
