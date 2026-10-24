# IAF Batch 1986 Reunion Management & Member Registry — AI Knowledge Base

Welcome to the **AI Knowledge Base** for the **Indian Air Force (IAF) Batch 1986 Reunion Management and Member Registry Application**.

This directory (`AI-KnowledgeBase/`) serves as the definitive reference manual and operational blueprint for AI coding agents and human engineers. It details the system architecture, domain models, business flows, security controls, database schema, and operational conventions implemented in this repository.

---

## 📖 Recommended Reading Order for AI Agents

When interacting with this codebase, AI agents should read the documentation in the following sequence:

1. **[PROJECT_CONTEXT.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/PROJECT_CONTEXT.md)**  
   *Read first.* Executive overview, core capabilities, architecture overview, and critical business rules.
2. **[AGENT_RULES.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/AGENT_RULES.md)**  
   *Read before editing code.* Standing instructions, architectural rules, code quality requirements, and anti-patterns.
3. **[TECH_STACK.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/TECH_STACK.md)**  
   Exact framework versions, packages, runtimes, and client-side libraries verified in source code.
4. **[PROJECT_STRUCTURE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/PROJECT_STRUCTURE.md)**  
   File tree, solution organization, project boundaries, and dependency flow.
5. **[ARCHITECTURE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ARCHITECTURE.md)**  
   Clean Architecture breakdown, dependency injection mapping, middleware pipeline, and data access design.
6. **[DATABASE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/DATABASE.md)**  
   SQL Server schema (`app` & `auth`), ER diagrams, tables, relationships, constraints, and seeding logic.
7. **[ENTITIES_AND_MODELS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ENTITIES_AND_MODELS.md)**  
   Domain entity definitions, DTOs, ViewModels, and enum specifications.
8. **[BUSINESS_FLOWS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/BUSINESS_FLOWS.md)**  
   End-to-end workflows: Excel/CSV member import, account activation, event registration, and payment verification.
9. **[AUTHENTICATION_AND_AUTHORIZATION.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/AUTHENTICATION_AND_AUTHORIZATION.md)**  
   ASP.NET Core Identity, cookie security (`__Host-`), RBAC (`Admin` vs `Member`), lockout policy, and CSP nonce handling.
10. **[API_REFERENCE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/API_REFERENCE.md)**  
    Complete catalog of MVC controller endpoints, route patterns, HTTP verbs, and access policies.
11. **[FRONTEND.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/FRONTEND.md)**  
    Razor view hierarchy, layouts, Bootstrap 5 UI, partials, LibMan assets, and client-side validation.
12. **[SERVICES_AND_INTEGRATIONS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/SERVICES_AND_INTEGRATIONS.md)**  
    Service interfaces, implementations, spreadsheet processing via ClosedXML/CsvHelper, and audit logging.
13. **[CONFIGURATION_AND_DEPLOYMENT.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CONFIGURATION_AND_DEPLOYMENT.md)**  
    Configuration keys, connection strings, migrations, and database initialization.
14. **[CODING_CONVENTIONS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CODING_CONVENTIONS.md)**  
    C# 14 / .NET 10 patterns, formatting, async practices, Roslyn analyzer requirements (`TreatWarningsAsErrors`).
15. **[SECURITY_AND_PERFORMANCE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/SECURITY_AND_PERFORMANCE.md)**  
    Security posture (CSP, CSRF, XSS, SSRF, SQLi), query optimization, and memory management.
16. **[TESTING.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/TESTING.md)**  
    Test architecture (xUnit + InMemoryDb), test suites, assertions, and manual execution instructions.
17. **[CHANGELOG.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CHANGELOG.md)**  
    Historical log of repository architectural decisions and major enhancements.

---

## 🗂️ Documentation Index

| File | Purpose |
| :--- | :--- |
| [README.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/README.md) | Documentation index and navigation map. |
| [PROJECT_CONTEXT.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/PROJECT_CONTEXT.md) | High-level system overview and quick-start context. |
| [AGENT_RULES.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/AGENT_RULES.md) | Mandatory instructions and constraints for AI agents. |
| [TECH_STACK.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/TECH_STACK.md) | Verified technology stack, SDKs, and NuGet/LibMan packages. |
| [PROJECT_STRUCTURE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/PROJECT_STRUCTURE.md) | Directory hierarchy and project dependency graph. |
| [ARCHITECTURE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ARCHITECTURE.md) | Architectural layers, patterns, and middleware pipeline. |
| [DATABASE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/DATABASE.md) | Complete database schema, tables, keys, and indexes. |
| [ENTITIES_AND_MODELS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/ENTITIES_AND_MODELS.md) | Detailed listing of Domain entities, DTOs, and Enums. |
| [API_REFERENCE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/API_REFERENCE.md) | MVC controller actions, parameters, HTTP methods, and auth rules. |
| [BUSINESS_FLOWS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/BUSINESS_FLOWS.md) | Step-by-step logic and sequence diagrams for core features. |
| [AUTHENTICATION_AND_AUTHORIZATION.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/AUTHENTICATION_AND_AUTHORIZATION.md) | Identity configuration, password policies, cookies, and roles. |
| [FRONTEND.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/FRONTEND.md) | View layout structure, Bootstrap 5 components, and LibMan assets. |
| [SERVICES_AND_INTEGRATIONS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/SERVICES_AND_INTEGRATIONS.md) | Application service interfaces, ClosedXML/CsvHelper, and AuditLogger. |
| [CONFIGURATION_AND_DEPLOYMENT.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CONFIGURATION_AND_DEPLOYMENT.md) | `appsettings.json`, connection strings, migrations, and seeding. |
| [CODING_CONVENTIONS.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CODING_CONVENTIONS.md) | Code styling, invariant formatting, Roslyn flags, and error handling. |
| [SECURITY_AND_PERFORMANCE.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/SECURITY_AND_PERFORMANCE.md) | Hardening details, CSP nonces, SQL tracking practices, and defenses. |
| [TESTING.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/TESTING.md) | Unit and integration test suites, commands, and assertions. |
| [CHANGELOG.md](file:///e:/ProjectTemplate/AI-KnowledgeBase/CHANGELOG.md) | Audit trail of codebase evolution. |
