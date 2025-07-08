# dnn-action-edit-item



<!-- Auto Generated Below -->


## Properties

| Property            | Attribute | Description       | Type   | Default     |
| ------------------- | --------- | ----------------- | ------ | ----------- |
| `item` _(required)_ | `item`    | The item to edit. | `Item` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-actions-bar](../../dnn-rm-actions-bar)
 - [dnn-rm-file-context-menu](../../context-menus/dnn-rm-file-context-menu)
 - [dnn-rm-folder-context-menu](../../context-menus/dnn-rm-folder-context-menu)

### Depends on

- dnn-modal
- [dnn-rm-edit-folder](../../dnn-rm-edit-folder)
- [dnn-rm-edit-file](../../dnn-rm-edit-file)

### Graph
```mermaid
graph TD;
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-edit-folder
  dnn-action-edit-item --> dnn-rm-edit-file
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
  dnn-rm-edit-file --> dnn-tabs
  dnn-rm-edit-file --> dnn-tab
  dnn-rm-edit-file --> dnn-input
  dnn-rm-edit-file --> dnn-textarea
  dnn-rm-edit-file --> dnn-button
  dnn-textarea --> dnn-fieldset
  dnn-rm-actions-bar --> dnn-action-edit-item
  dnn-rm-file-context-menu --> dnn-action-edit-item
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  style dnn-action-edit-item fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
