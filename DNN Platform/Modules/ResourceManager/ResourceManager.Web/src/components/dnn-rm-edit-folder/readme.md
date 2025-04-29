# dnn-rm-edit-folder

<!-- Auto Generated Below -->

## Properties

| Property                | Attribute   | Description                   | Type     | Default     |
| ----------------------- | ----------- | ----------------------------- | -------- | ----------- |
| `folderId` _(required)_ | `folder-id` | The ID of the folder to edit. | `number` | `undefined` |

## Events

| Event                 | Description                                                                                                        | Type                |
| --------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------- |
| `dnnRmFoldersChanged` | Fires when there is a possibility that some folders have changed. Can be used to force parts of the UI to refresh. | `CustomEvent<void>` |

## Dependencies

### Used by

- [dnn-action-edit-item](../actions/dnn-action-edit-item)

### Depends on

- dnn-tabs
- dnn-tab
- dnn-input
- dnn-permissions-grid
- dnn-button

### Graph

```mermaid
graph TD;
  dnn-rm-edit-folder --> dnn-tabs
  dnn-rm-edit-folder --> dnn-tab
  dnn-rm-edit-folder --> dnn-input
  dnn-rm-edit-folder --> dnn-permissions-grid
  dnn-rm-edit-folder --> dnn-button
  dnn-input --> dnn-fieldset
  dnn-permissions-grid --> dnn-checkbox
  dnn-permissions-grid --> dnn-button
  dnn-permissions-grid --> dnn-searchbox
  dnn-permissions-grid --> dnn-collapsible
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-edit-item --> dnn-rm-edit-folder
  style dnn-rm-edit-folder fill:#f9f,stroke:#333,stroke-width:4px
```

---

_Built with [StencilJS](https://stenciljs.com/)_
