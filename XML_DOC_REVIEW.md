# XML Documentation Review

**Repository:** `RSK.OpenIdentityServer`
**Date:** 2026-04-22
**Reviewer:** Automated documentation audit (compiler warnings + static heuristics)

---

## 1. Scope

Per the request, the following projects were **included** (shipping source projects):

| # | Project | Path |
|---|---------|------|
| 1 | `Open.IdentityServer.Storage` | `src/Storage/src/Open.IdentityServer.Storage.csproj` |
| 2 | `Open.IdentityModel` | `src/Open.IdentityModel/src/Open.IdentityModel.csproj` |
| 3 | `Open.IdentityServer.EntityFramework.Storage` | `src/EntityFramework.Storage/src/Open.IdentityServer.EntityFramework.Storage.csproj` |
| 4 | `Open.IdentityServer` | `src/Open.IdentityServer/src/Open.IdentityServer.csproj` |
| 5 | `Open.IdentityServer.EntityFramework` | `src/EntityFramework/src/Open.IdentityServer.EntityFramework.csproj` |
| 6 | `Open.IdentityServer.AspNetIdentity` | `src/AspNetIdentity/src/Open.IdentityServer.AspNetIdentity.csproj` |

All six have `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.

The following were **excluded** per the request:

- Unit & integration test projects (`*/test/**`, `*.UnitTests.csproj`, `*.IntegrationTests.csproj`, `TrimmableAnalysis.csproj`)
- `Host` / `ConsoleHost` / `host` projects
- EF Core migrations projects (`*/migrations/**`, e.g. `SqlServer.csproj`)
- Build automation projects (`*/build/build.csproj`)
- Samples & quickstarts (`samples/**`)

## 2. Methodology

1. Each shipping project was compiled with `dotnet build -c Release --no-incremental`. Because `GenerateDocumentationFile` is enabled, the Roslyn compiler emits:
   - **CS1591** — missing XML doc on a publicly-visible type/member.
   - **CS1574** — broken `cref`.
   - **CS1570 / CS1572 / CS1573 / CS1580 / CS1581 / CS1584 / CS1712 / CS1734** — malformed XML, unknown params, unresolved type params, etc.
2. In addition, a static scan (`/tmp/xmldocreview/scan.sh`) was run over all `.cs` files in the six projects to detect documentation *quality* defects that the compiler does **not** flag:
   - Empty `<summary></summary>` (single- or multi-line).
   - Empty `<param name="x"></param>`, `<typeparam name="T"></typeparam>`, `<returns></returns>`.
   - Stale `IdentityServer4.*` references in `cref` / `seealso` / prose.
   - `TODO` / `FIXME` / `TBD` inside doc comments.
   - GhostDoc-style placeholders (`%PARAM%`, etc.).

## 3. Headline numbers

| Category | Count | Severity |
|----------|------:|----------|
| CS1591 – missing XML docs on public members | **4** | High |
| CS1574 – broken `cref` | **4** | High |
| Empty `<summary></summary>` | **3** | High |
| Empty `<param name="…"></param>` | **189** | Medium |
| Empty `<typeparam name="…"></typeparam>` | **35** | Medium |
| Empty `<returns></returns>` | **478** | Low/Medium |
| Stale `IdentityServer4.*` references (crefs + prose) | **3** | Low |
| `TODO`/`FIXME`/`TBD` inside `///` | **0** | — |
| GhostDoc `%PARAM%` placeholders | **0** | — |

The codebase is in reasonable shape for presence of documentation — every
public member in five of six projects has a doc comment — but the **quality**
of that documentation has systemic gaps (hundreds of parameter tags that name
a parameter but provide no description).

---

## 4. High-severity findings (must fix)

### 4.1 CS1591 – Missing XML docs &nbsp;`[ ]`

Only one file triggers CS1591: `DataProtectedGrantData.cs`. The whole class
and its three public properties are public and undocumented.

**File:** `src/Open.IdentityServer/src/DataProtection/DataProtectedGrantData.cs`

**Current:**
```csharp
namespace Open.IdentityServer.Storage.Stores.DataProtection;

public class DataProtectedGrantData
{
    public int PersistentGrantDataContainerVersion { get; set; } = 1;
    public bool DataProtected { get; set; }
    public string Payload { get; set; }
}
```

**Proposed fix:**
```csharp
namespace Open.IdentityServer.Storage.Stores.DataProtection;

/// <summary>
/// Envelope used by <see cref="Open.IdentityServer.DataProtection.PersistentGrantSerializerDataProtectionDecorator"/>
/// to wrap a serialized persisted-grant payload together with the metadata needed
/// to determine whether the payload has been protected via ASP.NET Core Data Protection.
/// </summary>
public class DataProtectedGrantData
{
    /// <summary>
    /// Schema version of this envelope. Incremented when the shape of the payload changes
    /// in a way that is not backwards compatible.
    /// </summary>
    public int PersistentGrantDataContainerVersion { get; set; } = 1;

    /// <summary>
    /// <see langword="true"/> when <see cref="Payload"/> has been protected via
    /// <c>IDataProtector.Protect</c> and must be unprotected before deserialization;
    /// <see langword="false"/> when the payload is the raw serialized grant.
    /// </summary>
    public bool DataProtected { get; set; }

    /// <summary>
    /// The serialized grant payload (optionally data-protected — see <see cref="DataProtected"/>).
    /// </summary>
    public string Payload { get; set; }
}
```

> **Note:** The namespace declared in the file is
> `Open.IdentityServer.Storage.Stores.DataProtection`, which is misleading
> because the type lives in the `Open.IdentityServer` assembly, not
> `Open.IdentityServer.Storage`. Consider relocating to
> `Open.IdentityServer.DataProtection` alongside
> `PersistentGrantSerializerDataProtectionDecorator` for consistency. This is
> a code-structure concern, but it affects doc links.
Done
---

### 4.2 CS1574 – Broken `<see cref>` / `<seealso cref>`

Four broken references. All point to types that were renamed/renamespaced
during the IdentityServer4 → Open.IdentityServer fork but the crefs were not
updated.

| ✓ | File | Line | Broken cref | Actual FQN |
|---|------|-----:|-------------|------------|
| `[X]` | `src/Open.IdentityServer/src/DataProtection/PersistentGrantSerializerDataProtectionDecorator.cs` | 14 | `IdentityServer4.Stores.Serialization.IPersistentGrantSerializer` | `Open.IdentityServer.Stores.Serialization.IPersistentGrantSerializer` |
| `[X]` | `src/Open.IdentityServer/src/Endpoints/Results/ConsentPageResult.cs` | 17 | `Open.IdentityServer.Hosting.ReturnUrlResult` | `Open.IdentityServer.Endpoints.Results.ReturnUrlResult` |
| `[X]` | `src/Open.IdentityServer/src/Endpoints/Results/CustomRedirectResult.cs` | 17 | `Open.IdentityServer.Hosting.ReturnUrlResult` | `Open.IdentityServer.Endpoints.Results.ReturnUrlResult` |
| `[X]` | `src/Open.IdentityServer/src/Endpoints/Results/LoginPageResult.cs` | 17 | `Open.IdentityServer.Hosting.ReturnUrlResult` | `Open.IdentityServer.Endpoints.Results.ReturnUrlResult` |

**Proposed fixes** (one-line replacements):

```diff
- /// <seealso cref="IdentityServer4.Stores.Serialization.IPersistentGrantSerializer" />
+ /// <seealso cref="Open.IdentityServer.Stores.Serialization.IPersistentGrantSerializer" />
```

```diff
- /// <seealso cref="Open.IdentityServer.Hosting.ReturnUrlResult" />
+ /// <seealso cref="Open.IdentityServer.Endpoints.Results.ReturnUrlResult" />
```
(apply the second diff to each of the three `*PageResult.cs` / `*RedirectResult.cs` files).

Because these use a simple short-name or partially-qualified name would also
resolve, a cleaner alternative is `<seealso cref="ReturnUrlResult"/>` and
`<seealso cref="IPersistentGrantSerializer"/>` — the compiler will resolve
via `using`s and keep the doc robust to future namespace moves.

---

### 4.3 Empty `<summary>` elements

Three publicly-visible members ship with an empty `<summary/>`. The presence
of the tag suppresses CS1591, so the compiler did not catch these.

| ✓ | File | Line | Member | Proposed summary |
|---|------|-----:|--------|------------------|
| `[X]` | `src/Open.IdentityServer/src/Configuration/DependencyInjection/Options/InputLengthRestrictions.cs` | 7 | `class InputLengthRestrictions` | *"Maximum input length (in characters) that IdentityServer will accept for each well-known protocol parameter. Requests carrying a parameter longer than the configured limit are rejected as invalid."* |
| `[X]` | `src/Open.IdentityServer/src/Configuration/DependencyInjection/Options/LoggingOptions.cs` | 15 | `ICollection<string> TokenRequestSensitiveValuesFilter` | *"Names of token-endpoint request parameters whose values must be redacted when the request is written to the log (for example `client_secret`, `password`, `refresh_token`)."* |
| `[X]` | `src/Open.IdentityServer/src/Configuration/DependencyInjection/Options/LoggingOptions.cs` | 28 | `ICollection<string> AuthorizeRequestSensitiveValuesFilter` | *"Names of authorize-endpoint request parameters whose values must be redacted when the request is written to the log (for example `id_token_hint`)."* |

**Proposed fix** — `InputLengthRestrictions.cs`:
```diff
- /// <summary>
- ///
- /// </summary>
+ /// <summary>
+ /// Maximum input length (in characters) that IdentityServer will accept for each
+ /// well-known protocol parameter. Requests carrying a value longer than the configured
+ /// limit are rejected as invalid.
+ /// </summary>
  public class InputLengthRestrictions
```

**Proposed fix** — `LoggingOptions.cs`:
```diff
- /// <summary>
- ///
- /// </summary>
+ /// <summary>
+ /// Names of token-endpoint request parameters whose values must be redacted before the
+ /// request is written to the log (e.g. <c>client_secret</c>, <c>password</c>,
+ /// <c>client_assertion</c>, <c>refresh_token</c>, <c>device_code</c>).
+ /// </summary>
  public ICollection<string> TokenRequestSensitiveValuesFilter { get; set; } = …

- /// <summary>
- ///
- /// </summary>
+ /// <summary>
+ /// Names of authorize-endpoint request parameters whose values must be redacted before the
+ /// request is written to the log (e.g. <c>id_token_hint</c>).
+ /// </summary>
  public ICollection<string> AuthorizeRequestSensitiveValuesFilter { get; set; } = …
```

---

## 5. Medium-severity findings

### 5.1 Empty `<param>` tags (189 occurrences)

Every documented parameter has a `name=` attribute (so callers see the
method signature mirrored in IntelliSense), but the element body is empty.
This is the most prevalent doc-quality defect. The compiler cannot detect
it because it considers the tag "present".

**Pattern:**
```csharp
/// <summary>…</summary>
/// <param name="services"></param>
/// <param name="storeOptionsAction"></param>
/// <returns></returns>
public static IServiceCollection AddOperationalDbContext(
    this IServiceCollection services,
    Action<OperationalStoreOptions> storeOptionsAction = null)
```

**Top 10 offending files** (full list in Appendix A):

| # of empty `<param>` | File |
|---:|------|
| 12 | `src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs` |
| 11 | `src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs` |
| 11 | `src/AspNetIdentity/src/ProfileService.cs` |
| 10 | `src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs` |
| 8 | `src/Open.IdentityModel/src/Client/IntrospectionClient.cs` |
| 6 | `src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs` |
| 6 | `src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs` |
| 6 | `src/Open.IdentityModel/src/Client/Messages/Parameters.cs` |
| 5 | `src/Storage/src/Models/ClientClaim.cs` |
| 5 | `src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs` |

**Proposed remediation strategy:**

1. For each empty `<param>`, add a one-line description based on the
   parameter name and surrounding context. Example for
   `ServiceCollectionExtensions.AddOperationalDbContext`:
   ```csharp
   /// <param name="services">The DI service collection to register the operational
   /// DbContext into.</param>
   /// <param name="storeOptionsAction">Optional callback used to configure
   /// <see cref="OperationalStoreOptions"/> (connection string, default schema,
   /// token cleanup settings, etc.).</param>
   ```
2. For constructor parameters that just receive an injected service (the
   vast majority of empty params in `DefaultBackChannelLogoutService`,
   `EndSessionRequestValidator`, `TokenCleanupService`, etc.), the canonical
   phrasing is `"The &lt;T&gt; used by this component to …"`. A quick
   find-and-replace per file will clear dozens of warnings at once.
3. Where the parameter name already fully conveys the meaning (e.g.
   `<param name="cancellationToken">`), standardise on
   `"Propagates notification that operations should be cancelled."`.

See **Appendix A** for the complete file + line listing.

### 5.2 Empty `<typeparam>` tags (35 occurrences)

Same pattern, applied to generic type parameters. Because the vast majority
are boilerplate `<typeparam name="T"></typeparam>`, they should be replaced
by a meaningful description of the type argument.

| # | File |
|---:|------|
| 18 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs` |
| 2 | `src/Storage/src/Stores/Serialization/PersistentGrantSerializer.cs` |
| 2 | `src/Storage/src/Stores/Serialization/IPersistentGrantSerializer.cs` |
| 2 | `src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs` |
| 1 | `src/Open.IdentityServer/src/Stores/Default/ProtectedDataMessageStore.cs` |
| 1 | `src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs` |
| 1 | `src/Open.IdentityServer/src/Stores/Caching/CachingResourceStore.cs` |
| 1 | `src/Open.IdentityServer/src/Stores/Caching/CachingClientStore.cs` |
| 1 | `src/Open.IdentityServer/src/Services/Default/DefaultCache.cs` |
| 1 | `src/Open.IdentityServer/src/Extensions/ICacheExtensions.cs` |
| 1 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs` |
| 1 | `src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs` |
| 1 | `src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs` |
| 1 | `src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs` |
| 1 | `src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs` |

**Example** — `IPersistentGrantSerializer.cs`:

```diff
  /// <summary>Serializes the specified value.</summary>
- /// <typeparam name="T"></typeparam>
+ /// <typeparam name="T">Concrete grant model being serialized
+ /// (<see cref="Open.IdentityServer.Models.AuthorizationCode"/>,
+ /// <see cref="Open.IdentityServer.Models.RefreshToken"/>, etc.).</typeparam>
  /// <param name="value">The value.</param>
```

**Example** — `BuilderExtensions/Additional.cs` (18 offenders, all the
`Add*<T>` helpers):

```diff
- /// <typeparam name="T"></typeparam>
+ /// <typeparam name="T">Concrete implementation type of <see cref="IProfileService"/>
+ /// to register with the DI container.</typeparam>
```
(…adjust the interface reference per helper.)

Complete list in **Appendix B**.

### 5.3 Empty `<returns>` tags (478 occurrences)

These are near-universal in methods that return `Task`/`Task<T>`/`bool`/etc.
They are written as single-line `/// <returns></returns>` and appear to be a
stylistic leftover from IdentityServer4 (probably generated by GhostDoc). The
compiler doesn't flag them, but they produce a useless empty
`<returns/>` node in the generated `.xml` file, which in turn shows a blank
"Returns:" line in IntelliSense.

**Recommendation — one of:**

1. **Preferred:** populate them with a short description
   (e.g. `A task that resolves to the validated grant, or <c>null</c> when no
   matching grant exists.`). Tedious but that's what the user requested.
2. **Acceptable quick win:** delete the empty tag entirely. Methods returning
   `Task` with no value genuinely do not need a `<returns>`; for generic
   `Task<T>` the summary should describe the result. A regex sweep
   `s|///\s*<returns>\s*</returns>\r?\n||` removes them all in seconds.
3. Leave those on `void`/`Task` alone and populate those on `Task<T>`/non-void
   returning methods. This is the minimum that adds any real value to
   IntelliSense.

**Top 10 offending files** (full list in Appendix C):

| # of empty `<returns>` | File |
|---:|------|
| 26 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs` |
| 14 | `src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs` |
| 10 | `src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs` |
| 10 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs` |
| 10 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs` |
| 10 | `src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs` |
| 9 | `src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs` |
| 9 | `src/AspNetIdentity/src/ProfileService.cs` |
| 8 | `src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs` |
| 7 | `src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs` |

---

## 6. Low-severity findings

### 6.1 Stale `IdentityServer4` references in non-cref comments

Two `// ...` comments still point at the original IdentityServer4 issue
tracker. These are *not* XML docs, so they don't affect the generated doc
file, but they read awkwardly post-fork. Either delete or replace with an
explanatory note.

| ✓ | File | Line | Current |
|---|------|-----:|---------|
| `[X]` | `src/Open.IdentityServer/src/Configuration/DependencyInjection/ConfigureInternalCookieOptions.cs` | 47 | `// https://github.com/IdentityServer/IdentityServer4/issues/2595` |
| `[X]` | `src/AspNetIdentity/src/IdentityServerBuilderExtensions.cs` | 57 | `// https://github.com/IdentityServer/IdentityServer4/issues/2595` |

Proposed: replace with a one-line explanation ("we need `SameSite=None` so
the cookie is included on the authorize iframe request for silent renew").

### 6.2 Namespace inconsistency affecting documentation

- [X] `DataProtectedGrantData` is declared in the
  `Open.IdentityServer.Storage.Stores.DataProtection` namespace but lives in
  the `Open.IdentityServer` assembly. Moving it to
  `Open.IdentityServer.DataProtection` (next to its only consumer,
  `PersistentGrantSerializerDataProtectionDecorator`) would make the
  generated docs consistent.

---

## 7. Projects with a clean bill of health (zero issues found)

The following projects produced **zero** compiler-detected doc warnings and
**zero** scan-detected defects:

- `Open.IdentityServer.Storage` — only issue is empty `<returns>`/`<typeparam>` tags (see sections 5.2–5.3).
- `Open.IdentityModel` — same.
- `Open.IdentityServer.EntityFramework.Storage` — same.
- `Open.IdentityServer.EntityFramework` — same.
- `Open.IdentityServer.AspNetIdentity` — same.

All CS1591, CS1574 and empty-`<summary>` issues are concentrated in
`Open.IdentityServer`.

---

## 8. Recommended remediation order

1. [ ] **(15 min) Tier-1 fixes — high impact, tiny diff.**
   Apply the patches in §4.1, §4.2 and §4.3. Eliminates *all* compiler doc
   warnings and all empty summaries. Zero CS1591/CS1574 after this step.

2. [ ] **(1–2 h) Tier-2 fixes — empty `<param>` / `<typeparam>`.**
   Walk through the ~225 empty parameter tags using **Appendix A** and
   **Appendix B**. Most can be filled in with a single sentence; many
   repeat (e.g. `logger`, `options`, `cancellationToken`).

3. [ ] **(Decision needed) Tier-3 fixes — empty `<returns>`.**
   Pick one of the three strategies in §5.3. The minimum-effort path is a
   regex sweep to delete the empty tags; the maximum-value path is writing
   real descriptions.

4. [ ] **(Prevention) Enforce going forward.**
   Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (or at least
   `<WarningsAsErrors>CS1591;CS1570;CS1571;CS1572;CS1573;CS1574;CS1580;CS1581;CS1584;CS1587;CS1591;CS1658;CS1712;CS1734</WarningsAsErrors>`)
   to the shared `Directory.Build.props` so that future regressions fail the
   build. Optionally enable StyleCop analyzer `SA1623`/`SA1614`/`SA1616`
   which catch empty `<param>` / `<returns>` that the compiler ignores.

---

## Appendix A — Every empty `<param>` (189)

*Full list available at `/tmp/xmldocreview/empty_params.md` — included inline
below for archival.*

<details><summary>Expand 189 entries</summary>

- [X] src/Storage/src/Stores/Serialization/UnsupportedRefreshTokenException.cs:8:/// <param name="version"></param>
- [X] src/Storage/src/Models/ClientClaim.cs:40:    /// <param name="type"></param>
- [X] src/Storage/src/Models/ClientClaim.cs:41:    /// <param name="value"></param>
- [X] src/Storage/src/Models/ClientClaim.cs:51:    /// <param name="type"></param>
- [X] src/Storage/src/Models/ClientClaim.cs:52:    /// <param name="value"></param>
- [X] src/Storage/src/Models/ClientClaim.cs:53:    /// <param name="valueType"></param>
- [X] src/Storage/src/Extensions/PersistedGrantFilterExtensions.cs:18:    /// <param name="filter"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/JsonWebKeySetResponse.cs:18:    /// <param name="initializationData"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/MtlsEndpointAliases.cs:21:    /// <param name="json"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:23:    /// <param name="values"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:63:    /// <param name="values"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:98:    /// <param name="index"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:107:    /// <param name="name"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:117:    /// <param name="key"></param>
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:191:    /// <param name="additionalValues"></param>
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:21:    /// <param name="stringComparison"></param>
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:30:    /// <param name="issuerName"></param>
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:31:    /// <param name="expectedAuthority"></param>
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:46:    /// <param name="endpoint"></param>
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:47:    /// <param name="allowedAuthorities"></param>
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientJsonWebKeySetExtensions.cs:22:    /// <param name="address"></param>
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientPushedAuthorizationExtensions.cs:21:    /// <param name="request"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:22:    /// <param name="client"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:23:    /// <param name="options"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:31:    /// <param name="client"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:32:    /// <param name="options"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:66:    /// <param name="token"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:67:    /// <param name="tokenTypeHint"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:68:    /// <param name="parameters"></param>
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:69:    /// <param name="cancellationToken"></param>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:22:    /// <param name="services"></param>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:35:    /// <param name="services"></param>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:65:    /// <param name="services"></param>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:78:    /// <param name="services"></param>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:111:    /// <param name="services"></param>
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:86:    /// <param name="scopeNames"></param>
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:116:    /// <param name="scopeNames"></param>
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:143:    /// <param name="scopeNames"></param>
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:168:    /// <param name="model"></param>
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:169:    /// <param name="deviceCode"></param>
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:170:    /// <param name="userCode"></param>
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:191:    /// <param name="entity"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ApiResourceMappingExtensions.cs:13:    /// <param name="apiResourceEntity"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ApiResourceMappingExtensions.cs:41:    /// <param name="apiResourceModel"></param>
- [X] src/EntityFramework.Storage/src/Mappers/IdentityResourceMappingExtensions.cs:13:    /// <param name="identityResourceEntity"></param>
- [X] src/EntityFramework.Storage/src/Mappers/IdentityResourceMappingExtensions.cs:40:    /// <param name="identityResourceModel"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ScopeMappingExtensions.cs:13:    /// <param name="apiScopeEntity"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ScopeMappingExtensions.cs:40:    /// <param name="apiScopeModel"></param>
- [X] src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs:16:    /// <param name="secretList"></param>
- [X] src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs:43:    /// <param name="secretsModels"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ClientMappingExtensions.cs:18:    /// <param name="clientEntity"></param>
- [X] src/EntityFramework.Storage/src/Mappers/ClientMappingExtensions.cs:122:    /// <param name="clientModel"></param>
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:16:    /// <param name="propertyList"></param>
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:34:    /// <param name="propertyDictionary"></param>
- [X] src/EntityFramework.Storage/src/Mappers/UserClaimMappingExtensions.cs:32:    /// <param name="userClaims"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/IOperationalStoreNotification.cs:19:    /// <param name="persistedGrants"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/IOperationalStoreNotification.cs:26:    /// <param name="deviceCodes"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:29:    /// <param name="options"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:30:    /// <param name="persistedGrantDbContext"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:31:    /// <param name="operationalStoreNotification"></param>
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:32:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Stores/Default/DistributedCacheAuthorizationParametersMessageStore.cs:21:    /// <param name="distributedCache"></param>
- [X] src/Open.IdentityServer/src/Stores/Default/DistributedCacheAuthorizationParametersMessageStore.cs:22:    /// <param name="handleGenerationService"></param>
- [X] src/Open.IdentityServer/src/Stores/Default/ProtectedDataMessageStore.cs:36:    /// <param name="provider"></param>
- [X] src/Open.IdentityServer/src/Stores/Default/ProtectedDataMessageStore.cs:37:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Stores/Caching/CachingResourceStore.cs:45:    /// <param name="scopeCache"></param>
- [X] src/Open.IdentityServer/src/Models/Messages/Message.cs:18:    /// <param name="data"></param>
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:21:    /// <param name="resourceValidationResult"></param>
- [X] src/Open.IdentityServer/src/Extensions/X509CertificateExtensions.cs:17:    /// <param name="certificate"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:25:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:40:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:51:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:67:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:76:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:77:    /// <param name="clientId"></param>
- [X] src/Open.IdentityServer/src/DataProtection/DataProtectionException.cs:9:/// <param name="ex"></param>
- [X] src/Open.IdentityServer/src/DataProtection/DataProtectionException.cs:10:/// <param name="msg"></param>
- [X] src/Open.IdentityServer/src/Hosting/IdentityServerMiddleware.cs:41:    /// <param name="backChannelLogoutService"></param>
- [X] src/Open.IdentityServer/src/Hosting/MutualTlsEndpointMiddleware.cs:23:    /// <param name="next"></param>
- [X] src/Open.IdentityServer/src/Hosting/MutualTlsEndpointMiddleware.cs:24:    /// <param name="options"></param>
- [X] src/Open.IdentityServer/src/Hosting/MutualTlsEndpointMiddleware.cs:25:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:62:    /// <param name="keyMaterialService"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:109:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:127:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:149:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:150:    /// <param name="authorizationCode"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:224:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/UserInfoResponseGenerator.cs:113:    /// <param name="scopes"></param>
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/UserInfoResponseGenerator.cs:137:    /// <param name="resourceValidationResult"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultReplayCache.cs:19:    /// <param name="cache"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:55:    /// <param name="clock"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:56:    /// <param name="tools"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:57:    /// <param name="logoutNotificationService"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:58:    /// <param name="backChannelLogoutHttpClient"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:59:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:87:    /// <param name="requests"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:99:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:109:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:110:    /// <param name="data"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:120:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:136:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:152:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultEventService.cs:76:    /// <param name="evtType"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultRefreshTokenService.cs:49:    /// <param name="profile"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultRefreshTokenService.cs:154:    /// <param name="refreshToken"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:147:    /// <param name="principal"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:148:    /// <param name="properties"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:258:    /// <param name="sid"></param>
- [X] src/Open.IdentityServer/src/Services/Default/BackChannelLogoutHttpClient.cs:24:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Services/Default/BackChannelLogoutHttpClient.cs:25:    /// <param name="loggerFactory"></param>
- [X] src/Open.IdentityServer/src/Services/Default/BackChannelLogoutHttpClient.cs:35:    /// <param name="url"></param>
- [X] src/Open.IdentityServer/src/Services/Default/BackChannelLogoutHttpClient.cs:36:    /// <param name="payload"></param>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultTokenService.cs:74:    /// <param name="keyMaterialService"></param>
- [X] src/Open.IdentityServer/src/Services/IIdentityServerInteractionService.cs:59:    /// <param name="error"></param>
- [X] src/Open.IdentityServer/src/Services/IIdentityServerInteractionService.cs:60:    /// <param name="errorDescription"></param>
- [X] src/Open.IdentityServer/src/Services/IJwtRequestUriHttpClient.cs:14:    /// <param name="url"></param>
- [X] src/Open.IdentityServer/src/Services/IJwtRequestUriHttpClient.cs:15:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Services/IBackChannelLogoutHttpClient.cs:18:    /// <param name="url"></param>
- [X] src/Open.IdentityServer/src/Services/IBackChannelLogoutHttpClient.cs:19:    /// <param name="payload"></param>
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:14:    /// <param name="purpose"></param>
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:15:    /// <param name="handle"></param>
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:16:    /// <param name="expiration"></param>
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:24:    /// <param name="purpose"></param>
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:25:    /// <param name="handle"></param>
- [X] src/Open.IdentityServer/src/Validation/IEndSessionRequestValidator.cs:19:    /// <param name="parameters"></param>
- [X] src/Open.IdentityServer/src/Validation/IEndSessionRequestValidator.cs:20:    /// <param name="subject"></param>
- [X] src/Open.IdentityServer/src/Validation/IEndSessionRequestValidator.cs:27:    /// <param name="parameters"></param>
- [X] src/Open.IdentityServer/src/Validation/IAuthorizeRequestValidator.cs:19:    /// <param name="parameters"></param>
- [X] src/Open.IdentityServer/src/Validation/IAuthorizeRequestValidator.cs:20:    /// <param name="subject"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:179:    /// <param name="context"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:70:    /// <param name="context"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:71:    /// <param name="options"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:72:    /// <param name="tokenValidator"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:73:    /// <param name="uriValidator"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:74:    /// <param name="userSession"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:75:    /// <param name="logoutNotificationService"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:76:    /// <param name="endSessionMessageStore"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:77:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:183:    /// <param name="message"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:184:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:210:    /// <param name="request"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:21:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:63:    /// <param name="scopeContext"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:117:        /// <param name="parsedName"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:118:        /// <param name="parsedParameter"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:139:        /// <param name="error"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/MutualTlsSecretParser.cs:26:    /// <param name="options"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/MutualTlsSecretParser.cs:27:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/MutualTlsSecretParser.cs:42:    /// <param name="context"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/TokenRequestValidator.cs:56:    /// <param name="refreshTokenService"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:27:    /// <param name="scopeParser"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:78:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:79:    /// <param name="resourcesFromStore"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:80:    /// <param name="requestedScope"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:81:    /// <param name="result"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:149:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:150:    /// <param name="identity"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:165:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:166:    /// <param name="apiScope"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:181:    /// <param name="client"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/X509ThumbprintSecretValidator.cs:23:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Validation/Default/X509NameSecretValidator.cs:23:    /// <param name="logger"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValue.cs:17:    /// <param name="rawValue"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValue.cs:26:    /// <param name="rawValue"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValue.cs:27:    /// <param name="parsedName"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValue.cs:28:    /// <param name="parsedParameter"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ResourceValidationResult.cs:26:    /// <param name="resources"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ResourceValidationResult.cs:36:    /// <param name="resources"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ResourceValidationResult.cs:37:    /// <param name="parsedScopeValues"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ResourceValidationResult.cs:72:    /// <param name="scopeValues"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValidationError.cs:17:    /// <param name="rawValue"></param>
- [X] src/Open.IdentityServer/src/Validation/Models/ParsedScopeValidationError.cs:18:    /// <param name="error"></param>
- [X] src/Open.IdentityServer/src/Validation/IDeviceAuthorizationRequestValidator.cs:18:    /// <param name="parameters"></param>
- [X] src/Open.IdentityServer/src/Validation/IDeviceAuthorizationRequestValidator.cs:19:    /// <param name="clientValidationResult"></param>
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:114:    /// <param name="builder"></param>
- [X] src/EntityFramework/src/TokenCleanupHost.cs:31:    /// <param name="serviceProvider"></param>
- [X] src/EntityFramework/src/TokenCleanupHost.cs:32:    /// <param name="options"></param>
- [X] src/EntityFramework/src/TokenCleanupHost.cs:33:    /// <param name="logger"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:82:    /// <param name="context"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:83:    /// <param name="subjectId"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:97:    /// <param name="context"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:98:    /// <param name="user"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:109:    /// <param name="user"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:136:    /// <param name="context"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:137:    /// <param name="subjectId"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:155:    /// <param name="context"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:156:    /// <param name="user"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:166:    /// <param name="user"></param>
- [X] src/AspNetIdentity/src/ProfileService.cs:176:    /// <param name="subjectId"></param>


</details>

## Appendix B — Every empty `<typeparam>` (35)

<details><summary>Expand 35 entries</summary>

- [X] src/Storage/src/Stores/Serialization/IPersistentGrantSerializer.cs:16:    /// <typeparam name="T"></typeparam>
- [X] src/Storage/src/Stores/Serialization/IPersistentGrantSerializer.cs:24:    /// <typeparam name="T"></typeparam>
- [X] src/Storage/src/Stores/Serialization/PersistentGrantSerializer.cs:34:    /// <typeparam name="T"></typeparam>
- [X] src/Storage/src/Stores/Serialization/PersistentGrantSerializer.cs:45:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs:111:    /// <typeparam name="T"></typeparam>
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:110:    /// <typeparam name="T"></typeparam>
- [X] src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs:17:    /// <typeparam name="TSecret"></typeparam>
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:17:    /// <typeparam name="TProperty"></typeparam>
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:40:        /// <typeparam name="TProperty"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:26:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:40:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:64:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:78:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:92:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:106:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:120:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:135:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:149:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:203:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:217:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:261:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:275:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:289:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:317:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:345:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:433:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:447:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:102:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Stores/Default/ProtectedDataMessageStore.cs:18:/// <typeparam name="TModel"></typeparam>
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:18:/// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Stores/Caching/CachingResourceStore.cs:19:/// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Stores/Caching/CachingClientStore.cs:17:/// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Extensions/ICacheExtensions.cs:21:    /// <typeparam name="T"></typeparam>
- [X] src/Open.IdentityServer/src/Services/Default/DefaultCache.cs:14:/// <typeparam name="T"></typeparam>
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:113:    /// <typeparam name="T"></typeparam>


</details>

## Appendix C — Every empty `<returns>` (478)

<details><summary>Expand 478 entries</summary>

- [X] src/Storage/src/Stores/IAuthorizationCodeStore.cs:19: empty <returns> (same line)
- [X] src/Storage/src/Stores/IAuthorizationCodeStore.cs:26: empty <returns> (same line)
- [X] src/Storage/src/Stores/IAuthorizationCodeStore.cs:33: empty <returns> (same line)
- [X] src/Storage/src/Stores/IPersistedGrantStore.cs:20: empty <returns> (same line)
- [X] src/Storage/src/Stores/IPersistedGrantStore.cs:27: empty <returns> (same line)
- [X] src/Storage/src/Stores/IPersistedGrantStore.cs:34: empty <returns> (same line)
- [X] src/Storage/src/Stores/IPersistedGrantStore.cs:41: empty <returns> (same line)
- [X] src/Storage/src/Stores/IPersistedGrantStore.cs:48: empty <returns> (same line)
- [X] src/Storage/src/Stores/Serialization/IPersistentGrantSerializer.cs:18: empty <returns> (same line)
- [X] src/Storage/src/Stores/Serialization/IPersistentGrantSerializer.cs:26: empty <returns> (same line)
- [X] src/Storage/src/Stores/Serialization/PersistentGrantSerializer.cs:36: empty <returns> (same line)
- [X] src/Storage/src/Stores/Serialization/PersistentGrantSerializer.cs:47: empty <returns> (same line)
- [X] src/Storage/src/Stores/IUserConsentStore.cs:19: empty <returns> (same line)
- [X] src/Storage/src/Stores/IUserConsentStore.cs:27: empty <returns> (same line)
- [X] src/Storage/src/Stores/IUserConsentStore.cs:35: empty <returns> (same line)
- [X] src/Storage/src/Stores/IReferenceTokenStore.cs:19: empty <returns> (same line)
- [X] src/Storage/src/Stores/IReferenceTokenStore.cs:26: empty <returns> (same line)
- [X] src/Storage/src/Stores/IReferenceTokenStore.cs:33: empty <returns> (same line)
- [X] src/Storage/src/Stores/IReferenceTokenStore.cs:41: empty <returns> (same line)
- [X] src/Storage/src/Stores/IRefreshTokenStore.cs:19: empty <returns> (same line)
- [X] src/Storage/src/Stores/IRefreshTokenStore.cs:27: empty <returns> (same line)
- [X] src/Storage/src/Stores/IRefreshTokenStore.cs:34: empty <returns> (same line)
- [X] src/Storage/src/Stores/IRefreshTokenStore.cs:41: empty <returns> (same line)
- [X] src/Storage/src/Stores/IRefreshTokenStore.cs:49: empty <returns> (same line)
- [X] src/Storage/src/Stores/IDeviceFlowStore.cs:21: empty <returns> (same line)
- [X] src/Storage/src/Stores/IDeviceFlowStore.cs:28: empty <returns> (same line)
- [X] src/Storage/src/Models/RefreshToken.cs:116: empty <returns> (same line)
- [X] src/Storage/src/IdentityServerUser.cs:62: empty <returns> (same line)
- [X] src/Storage/src/Services/ICorsPolicyService.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/CryptoRandom.cs:40: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/CryptoRandom.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Base64Url.cs:17: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Base64Url.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Principal.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Principal.cs:36: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Jwk/JwkExtensions.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Identity.cs:40: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Identity.cs:52: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/DiscoveryCache.cs:56: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/RequestUrl.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/DiscoveryEndpoint.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs:114: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs:131: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/ProtocolResponse.cs:240: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/TokenIntrospectionResponse.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/JsonWebKeySetResponse.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/AuthorizeResponse.cs:194: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/UserInfoResponse.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/DiscoveryDocumentResponse.cs:128: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/DiscoveryDocumentResponse.cs:140: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/DiscoveryDocumentResponse.cs:152: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/DiscoveryDocumentResponse.cs:163: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:108: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Messages/Parameters.cs:118: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:71: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:89: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:109: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:131: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:152: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/TokenClient.cs:171: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/BasicAuthenticationOAuthHeaderValue.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/BasicAuthenticationHeaderValue.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:32: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/StringComparisonAuthorityValidationStrategy.cs:48: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/IDiscoveryCache.cs:25: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/IAuthorityValidationStrategy.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/IAuthorityValidationStrategy.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenIntrospectionExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientJsonWebKeySetExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientJsonWebKeySetExtensions.cs:39: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientPushedAuthorizationExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:22: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:65: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:81: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:102: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:114: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/JsonElementExtensions.cs:132: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientDiscoveryExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientDiscoveryExtensions.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/RequestUrlExtensions.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/RequestUrlExtensions.cs:46: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/RequestUrlExtensions.cs:107: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:45: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:62: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:86: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:110: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:133: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:158: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:180: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRequestExtensions.cs:200: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientBackchannelAuthenticationExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientUserInfoExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientDynamicRegistrationExtensions.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientTokenRevocationExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/Extensions/HttpClientDeviceFlowExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityModel/src/Client/IntrospectionClient.cs:70: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:24: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:37: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:67: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:80: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Configuration/ServiceCollectionExtensions.cs:112: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:50: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:87: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:117: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:144: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/ResourceStore.cs:170: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:62: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:74: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:90: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:107: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:138: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:171: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Stores/DeviceFlowStore.cs:192: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs:24: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Mappers/SecretMappingExtensions.cs:50: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:24: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Mappers/PropertyMappingExtensions.cs:41: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/TokenCleanup/IOperationalStoreNotification.cs:20: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/TokenCleanup/IOperationalStoreNotification.cs:27: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:51: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:70: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs:103: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/DbContexts/PersistedGrantDbContext.cs:76: empty <returns> (same line)
- [X] src/EntityFramework.Storage/src/Interfaces/IPersistedGrantDbContext.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/IdentityServerApplicationBuilderExtensions.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/CryptoHelper.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/CryptoHelper.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/CryptoHelper.cs:47: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/CryptoHelper.cs:63: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/CryptoHelper.cs:82: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs:60: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs:72: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:28: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:42: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:55: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:66: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:80: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:94: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:108: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:122: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:137: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:178: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:191: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:205: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:219: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:233: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:249: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:263: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:277: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:291: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:304: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:319: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:332: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:347: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:362: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:399: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:435: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Additional.cs:449: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:43: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:59: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:78: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:106: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:120: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:146: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:201: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:230: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:249: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Core.cs:262: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:25: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:39: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:53: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:67: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:81: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:95: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:109: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:123: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:149: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/InMemory.cs:163: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:73: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:121: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:134: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:147: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:161: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:200: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:214: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:235: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs:256: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/TokenEndpoint.cs:58: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/UserInfoEndpoint.cs:51: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/DeviceAuthorizationEndpoint.cs:50: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/TokenRevocationEndpoint.cs:58: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/IntrospectionEndpoint.cs:57: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/LoginPageResult.cs:38: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/TokenRevocationErrorResult.cs:40: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/IntrospectionResult.cs:42: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/DiscoveryDocumentResult.cs:52: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/JsonWebKeysResult.cs:52: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/ConsentPageResult.cs:38: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/ReturnUrlResult.cs:89: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/CustomRedirectResult.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/EndSessionResult.cs:64: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Endpoints/Results/StatusCodeResult.cs:48: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserResourceOwnerPasswordValidator.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserProfileService.cs:45: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserProfileService.cs:69: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/IdentityServerBuilderExtensions.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserStore.cs:35: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserStore.cs:57: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserStore.cs:67: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserStore.cs:78: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Test/TestUserStore.cs:92: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IAuthorizationParametersMessageStore.cs:27: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IAuthorizationParametersMessageStore.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultRefreshTokenStore.cs:38: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultRefreshTokenStore.cs:49: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultRefreshTokenStore.cs:59: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultRefreshTokenStore.cs:69: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultRefreshTokenStore.cs:80: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultUserConsentStore.cs:44: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultUserConsentStore.cs:56: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultUserConsentStore.cs:78: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultAuthorizationCodeStore.cs:39: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultAuthorizationCodeStore.cs:49: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultAuthorizationCodeStore.cs:59: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultReferenceTokenStore.cs:38: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultReferenceTokenStore.cs:48: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultReferenceTokenStore.cs:58: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultReferenceTokenStore.cs:69: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:81: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:92: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:127: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:147: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:175: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Default/DefaultGrantStore.cs:187: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/InMemory/InMemoryDeviceFlowStore.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/InMemory/InMemoryDeviceFlowStore.cs:93: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/InMemory/InMemorySigningCredentialsStore.cs:30: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/InMemory/InMemoryValidationKeysStore.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/ISigningCredentialStore.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IConsentMessageStore.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IConsentMessageStore.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IValidationKeysStore.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/Caching/CachingCorsPolicyService.cs:66: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Stores/IMessageStore.cs:27: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Models/DeviceFlowInteractionResult.cs:39: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ClientExtensions.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IClientStoreExtensions.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextExtensions.cs:84: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextExtensions.cs:95: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextExtensions.cs:105: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextExtensions.cs:116: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextExtensions.cs:133: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:22: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:36: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:68: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ResourceExtensions.cs:82: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/X509CertificateExtensions.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/TokenExtensions.cs:31: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IResourceStoreExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IResourceStoreExtensions.cs:96: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IResourceStoreExtensions.cs:107: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IResourceStoreExtensions.cs:120: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IResourceStoreExtensions.cs:134: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:23: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:45: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:61: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:72: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:88: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:100: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:117: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:134: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:145: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:156: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:172: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:184: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/PrincipalExtensions.cs:195: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IUserSessionExtensions.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ProfileDataRequestContextExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/ICacheExtensions.cs:27: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:42: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/AuthenticationPropertiesExtensions.cs:52: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/IdentityServerToolsExtensions.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextAuthenticationExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Extensions/HttpContextAuthenticationExtensions.cs:36: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/DataProtection/PersistentGrantSerializerDataProtectionDecorator.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/DataProtection/PersistentGrantSerializerDataProtectionDecorator.cs:53: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/IdentityServerUser.cs:63: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/IdentityServerMiddleware.cs:42: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/LocalApiAuthentication/LocalApiAuthenticationExtensions.cs:24: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/LocalApiAuthentication/LocalApiAuthenticationExtensions.cs:60: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/LocalApiAuthentication/LocalApiAuthenticationExtensions.cs:69: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/LocalApiAuthentication/LocalApiAuthenticationExtensions.cs:79: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/LocalApiAuthentication/LocalApiAuthenticationExtensions.cs:90: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/IEndpointResult.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/IEndpointHandler.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Hosting/IEndpointRouter.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Events/Infrastructure/Event.cs:39: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Events/Infrastructure/Event.cs:129: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Infrastructure/DistributedCacheStateDataFormatter.cs:43: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Infrastructure/DistributedCacheStateDataFormatter.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Infrastructure/DistributedCacheStateDataFormatter.cs:82: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Infrastructure/DistributedCacheStateDataFormatter.cs:93: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Logging/LogSerializer.cs:30: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/IAuthorizeInteractionResponseGenerator.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/ITokenRevocationResponseGenerator.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeInteractionResponseGenerator.cs:67: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeInteractionResponseGenerator.cs:121: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeInteractionResponseGenerator.cs:230: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:86: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:110: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:122: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:134: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:201: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:255: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:319: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:331: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:355: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenResponseGenerator.cs:442: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/TokenRevocationResponseGenerator.cs:59: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/DeviceAuthorizationResponseGenerator.cs:70: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/IntrospectionResponseGenerator.cs:51: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/IntrospectionResponseGenerator.cs:99: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:85: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:110: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:128: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:151: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/AuthorizeResponseGenerator.cs:225: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/UserInfoResponseGenerator.cs:58: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/UserInfoResponseGenerator.cs:114: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/Default/UserInfoResponseGenerator.cs:138: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/IUserInfoResponseGenerator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/IIntrospectionResponseGenerator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/ITokenResponseGenerator.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/IAuthorizeResponseGenerator.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/ResponseHandling/IDeviceAuthorizationResponseGenerator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IProfileService.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IProfileService.cs:27: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserCodeGenerator.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IRefreshTokenService.cs:22: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IDeviceFlowCodeService.cs:22: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultDeviceFlowCodeService.cs:33: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultDeviceFlowCodeService.cs:47: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultDeviceFlowCodeService.cs:57: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultDeviceFlowCodeService.cs:68: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultDeviceFlowCodeService.cs:78: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserCodeService.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:88: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:111: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultBackChannelLogoutService.cs:121: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultEventService.cs:60: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultEventService.cs:77: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultEventService.cs:112: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultRefreshTokenService.cs:68: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultRefreshTokenService.cs:155: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DistributedDeviceFlowThrottlingService.cs:47: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultConsentService.cs:149: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:149: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:180: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:191: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:202: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:219: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:277: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultUserSession.cs:298: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/NumericUserCodeGenerator.cs:36: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultClaimsService.cs:257: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/ReturnUrlParser.cs:31: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultProfileService.cs:36: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultProfileService.cs:51: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultCorsPolicyService.cs:52: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultHandleGenerationService.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/BackChannelLogoutHttpClient.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/Default/DefaultCache.cs:57: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IKeyMaterialService.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IKeyMaterialService.cs:28: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IKeyMaterialService.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IPersistedGrantService.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IPersistedGrantService.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IDeviceFlowThrottlingService.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/InMemory/InMemoryCorsPolicyService.cs:44: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IReturnUrlParser.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IIdentityServerInteractionService.cs:43: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserSession.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserSession.cs:32: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserSession.cs:44: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserSession.cs:50: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IHandleGenerationService.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IDeviceFlowInteractionService.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IDeviceFlowInteractionService.cs:27: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IJwtRequestUriHttpClient.cs:16: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IUserCodeService.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IConsentService.cs:35: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/ICache.cs:30: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IBackChannelLogoutHttpClient.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:17: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Services/IReplayCache.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IEndSessionRequestValidator.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IEndSessionRequestValidator.cs:28: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IApiSecretValidator.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IAuthorizeRequestValidator.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IClientSecretValidator.cs:19: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IDeviceCodeValidator.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/ITokenValidator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/ITokenValidator.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IUserInfoRequestValidator.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/BearerTokenUsageValidator.cs:34: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/BearerTokenUsageValidator.cs:62: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/BearerTokenUsageValidator.cs:95: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/TokenRevocationRequestValidator.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:29: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:61: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:76: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:118: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:141: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:180: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:214: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultClientConfigurationValidator.cs:239: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/EndSessionRequestValidator.cs:185: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/IntrospectionRequestValidator.cs:37: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/JwtRequestValidator.cs:90: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/JwtRequestValidator.cs:149: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/JwtRequestValidator.cs:161: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/JwtRequestValidator.cs:197: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultScopeParser.cs:64: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/UserInfoRequestValidator.cs:45: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/MutualTlsSecretParser.cs:43: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/ClientSecretValidator.cs:47: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/SecretValidator.cs:43: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/NotSupportedResouceOwnerCredentialValidator.cs:32: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/TokenRequestValidator.cs:96: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DeviceCodeValidator.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:82: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:151: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:167: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/DefaultResourceValidator.cs:182: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/ExtensionGrantValidator.cs:44: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/ExtensionGrantValidator.cs:54: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/ApiSecretValidator.cs:47: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/NopClientConfigurationValidator.cs:15: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/StrictRedirectUriValidator.cs:25: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/SecretParser.cs:38: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Default/SecretParser.cs:72: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/Models/ResourceValidationResult.cs:73: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IDeviceAuthorizationRequestValidator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/ISecretsListParser.cs:26: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/ITokenRevocationRequestValidator.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IClientConfigurationValidator.cs:18: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/IIntrospectionRequestValidator.cs:21: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/Validation/ITokenRequestValidator.cs:20: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/IdentityServerTools.cs:45: empty <returns> (same line)
- [X] src/Open.IdentityServer/src/IdentityServerTools.cs:71: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:29: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:43: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:62: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:81: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:95: empty <returns> (same line)
- [X] src/EntityFramework/src/IdentityServerEntityFrameworkBuilderExtensions.cs:115: empty <returns> (same line)
- [X] src/EntityFramework/src/Services/CorsPolicyService.cs:40: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ResourceOwnerPasswordValidator.cs:48: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:70: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:84: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:99: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:110: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:124: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:138: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:157: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:167: empty <returns> (same line)
- [X] src/AspNetIdentity/src/ProfileService.cs:177: empty <returns> (same line)
- [X] src/AspNetIdentity/src/IdentityServerBuilderExtensions.cs:29: empty <returns> (same line)
- [ ] src/AspNetIdentity/src/SecurityStampValidatorCallback.cs:21: empty <returns> (same line)


</details>

---

*End of report.*

