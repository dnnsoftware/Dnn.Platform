# dnn-resource-manager

<!-- Auto Generated Below -->

## Properties

| Property                | Attribute   | Description           | Type     | Default     |
| ----------------------- | ----------- | --------------------- | -------- | ----------- |
| `moduleId` _(required)_ | `module-id` | The ID of the module. | `number` | `undefined` |

## Dependencies

### Depends on

- [dnn-rm-top-bar](../dnn-rm-top-bar)
- dnn-vertical-splitview
- [dnn-rm-left-pane](../dnn-rm-left-pane)
- [dnn-rm-right-pane](../dnn-rm-right-pane)
- dnn-modal
- [dnn-rm-folder-mappings](../dnn-rm-folder-mappings)

### Graph

```mermaid
graph TD;
  dnn-resource-manager --> dnn-rm-top-bar
  dnn-resource-manager --> dnn-vertical-splitview
  dnn-resource-manager --> dnn-rm-left-pane
  dnn-resource-manager --> dnn-rm-right-pane
  dnn-resource-manager --> dnn-modal
  dnn-resource-manager --> dnn-rm-folder-mappings
  dnn-rm-top-bar --> dnn-searchbox
  dnn-rm-left-pane --> dnn-rm-folder-list
  dnn-rm-folder-list --> dnn-collapsible
  dnn-rm-folder-list --> dnn-rm-folder-context-menu
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  dnn-rm-folder-context-menu --> dnn-action-move-items
  dnn-rm-folder-context-menu --> dnn-action-delete-items
  dnn-rm-folder-context-menu --> dnn-action-unlink-items
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-input
  dnn-rm-create-folder --> dnn-select
  dnn-rm-create-folder --> dnn-button
  dnn-input --> dnn-fieldset
  dnn-select --> dnn-fieldset
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-edit-folder
  dnn-action-edit-item --> dnn-rm-edit-file
  dnn-rm-edit-folder --> dnn-tabs
  dnn-rm-edit-folder --> dnn-tab
  dnn-rm-edit-folder --> dnn-input
  dnn-rm-edit-folder --> dnn-permissions-grid
  dnn-rm-edit-folder --> dnn-button
  dnn-permissions-grid --> dnn-checkbox
  dnn-permissions-grid --> dnn-button
  dnn-permissions-grid --> dnn-searchbox
  dnn-permissions-grid --> dnn-collapsible
  dnn-rm-edit-file --> dnn-tabs
  dnn-rm-edit-file --> dnn-tab
  dnn-rm-edit-file --> dnn-input
  dnn-rm-edit-file --> dnn-textarea
  dnn-rm-edit-file --> dnn-button
  dnn-textarea --> dnn-fieldset
  dnn-action-move-items --> dnn-modal
  dnn-action-move-items --> dnn-rm-move-items
  dnn-rm-move-items --> dnn-rm-folder-list
  dnn-rm-move-items --> dnn-rm-progress-bar
  dnn-rm-move-items --> dnn-button
  dnn-action-delete-items --> dnn-modal
  dnn-action-delete-items --> dnn-rm-delete-items
  dnn-rm-delete-items --> dnn-rm-progress-bar
  dnn-rm-delete-items --> dnn-button
  dnn-action-unlink-items --> dnn-modal
  dnn-action-unlink-items --> dnn-rm-unlink-items
  dnn-rm-unlink-items --> dnn-rm-progress-bar
  dnn-rm-unlink-items --> dnn-button
  dnn-rm-folder-list-item --> dnn-collapsible
  dnn-rm-folder-list-item --> dnn-rm-folder-context-menu
  dnn-rm-folder-list-item --> dnn-treeview-item
  dnn-rm-folder-list-item --> dnn-rm-folder-list-item
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-right-pane --> dnn-rm-actions-bar
  dnn-rm-right-pane --> dnn-rm-files-pane
  dnn-rm-right-pane --> dnn-rm-status-bar
  dnn-rm-actions-bar --> dnn-vertical-overflow-menu
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-actions-bar --> dnn-action-upload-file
  dnn-rm-actions-bar --> dnn-action-edit-item
  dnn-rm-actions-bar --> dnn-action-move-items
  dnn-rm-actions-bar --> dnn-action-delete-items
  dnn-rm-actions-bar --> dnn-action-unlink-items
  dnn-rm-actions-bar --> dnn-action-copy-url
  dnn-rm-actions-bar --> dnn-action-open-file
  dnn-rm-actions-bar --> dnn-action-download-item
  dnn-rm-actions-bar --> dnn-collapsible
  dnn-action-upload-file --> dnn-modal
  dnn-action-upload-file --> dnn-rm-upload-file
  dnn-rm-upload-file --> dnn-checkbox
  dnn-rm-upload-file --> dnn-dropzone
  dnn-rm-upload-file --> dnn-rm-queued-file
  dnn-rm-queued-file --> dnn-button
  dnn-rm-files-pane --> dnn-rm-items-listview
  dnn-rm-files-pane --> dnn-rm-items-cardview
  dnn-rm-items-listview --> dnn-rm-folder-context-menu
  dnn-rm-items-listview --> dnn-rm-file-context-menu
  dnn-rm-items-listview --> dnn-collapsible
  dnn-rm-file-context-menu --> dnn-action-edit-item
  dnn-rm-file-context-menu --> dnn-action-move-items
  dnn-rm-file-context-menu --> dnn-action-delete-items
  dnn-rm-file-context-menu --> dnn-action-open-file
  dnn-rm-file-context-menu --> dnn-action-download-item
  dnn-rm-file-context-menu --> dnn-action-copy-url
  dnn-rm-items-cardview --> dnn-collapsible
  dnn-rm-items-cardview --> dnn-rm-folder-context-menu
  dnn-rm-items-cardview --> dnn-rm-file-context-menu
  dnn-rm-folder-mappings --> dnn-button
  style dnn-resource-manager fill:#f9f,stroke:#333,stroke-width:4px
```

---

_Built with [StencilJS](https://stenciljs.com/)_
