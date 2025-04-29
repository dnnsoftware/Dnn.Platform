# dnn-action-create-folder

<!-- Auto Generated Below -->

## Properties

| Property         | Attribute          | Description | Type     | Default     |
| ---------------- | ------------------ | ----------- | -------- | ----------- |
| `parentFolderId` | `parent-folder-id` |             | `number` | `undefined` |

## Dependencies

### Used by

- [dnn-rm-actions-bar](../../dnn-rm-actions-bar)
- [dnn-rm-folder-context-menu](../../context-menus/dnn-rm-folder-context-menu)

### Depends on

- dnn-modal
- [dnn-rm-create-folder](../../dnn-rm-create-folder)

### Graph

```mermaid
graph TD;
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-input
  dnn-rm-create-folder --> dnn-select
  dnn-rm-create-folder --> dnn-button
  dnn-input --> dnn-fieldset
  dnn-select --> dnn-fieldset
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  style dnn-action-create-folder fill:#f9f,stroke:#333,stroke-width:4px
```

---

_Built with [StencilJS](https://stenciljs.com/)_
