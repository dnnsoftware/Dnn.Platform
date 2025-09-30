### Enhancement: Priority-based IPageService for page metadata, head tags, and messages

#### Summary
Add a simple, priority-driven API for setting page title/description/keywords/canonical URL, injecting head tags and meta tags, and collecting user-visible messages. This formalizes page composition logic and decouples it from the rendering pipeline, supporting both the current WebForms pipeline and the new MVC pipeline.

#### Motivation
- **Unify page composition**: Provide a single abstraction for metadata, head elements, and messages across pipelines.
- **Deterministic ordering**: Ensure consistent output using explicit priorities.
- **Progress toward MVC/Core**: Keeps page state management out of WebForms, easing the hybrid transition.
- **Testability**: A small, mockable surface that’s easy to unit test.

#### Scope
- Interfaces and models:
  - `IPageService`
  - `PageMessage`, `PageMessageType`
  - `PageMeta`
  - `PageTag`
  - `PagePriority`


#### Proposed API (already defined)
- **Title/SEO**
  - `void SetTitle(string value, int priority = PagePriority.Default)`
  - `void SetDescription(string value, int priority = PagePriority.Default)`
  - `void SetKeyWords(string value, int priority = PagePriority.Default)`
  - `void SetCanonicalLinkUrl(string value, int priority = PagePriority.Default)`
  - `string GetTitle()`, `string GetDescription()`, `string GetKeyWords()`, `string GetCanonicalLinkUrl()`
- **Head content**
  - `void AddToHead(PageTag tagItem)`
  - `List<PageTag> GetHeadTags()`
  - `void AddMeta(PageMeta metaItem)`
  - `List<PageMeta> GetMetaTags()`
- **Messages**
  - `void AddMessage(PageMessage messageItem)`
  - `List<PageMessage> GetMessages()`
- **Maintenance**
  - `void Clear()`
- **Priority model**
  - `PagePriority.Site = 10`, `PagePriority.Page = 20`, `PagePriority.Default = 100`, `PagePriority.Module = 200`
- **Message model**
  - `PageMessageType`: `Success`, `Warning`, `Error`, `Info`
  - `PageMessage`: `Heading`, `Message`, `MessageType`, `IconSrc`, `Priority`
- **Meta/head models**
  - `PageMeta`: `Name`, `Content`, `Priority`
  - `PageTag`: `Value`, `Priority`

#### Example usage
```csharp
pageService.SetTitle("Products", PagePriority.Page);
pageService.SetDescription("Browse our product catalog.", PagePriority.Page);
pageService.SetCanonicalLinkUrl("https://www.mysite.com/products", PagePriority.Page);

pageService.AddMeta(new PageMeta("viewport", "width=device-width, initial-scale=1.0", PagePriority.Site));
pageService.AddToHead(new PageTag("<link rel=\"stylesheet\" href=\"/css/catalog.css\" />", PagePriority.Page));

pageService.AddMessage(new PageMessage(
    "Saved", "Your product was updated.", PageMessageType.Success, "", PagePriority.Default));
```

#### Rendering contract (follow-up implementation)
- The renderer (WebForms skin, MVC layout) must:
  - Pick the highest-priority values for title/description/keywords/canonical link.
  - Render `GetMetaTags()` and `GetHeadTags()` in ascending `Priority` order.
  - Render `GetMessages()` in ascending `Priority` order, with styling determined by `PageMessageType`.




