# dnn-action-move-items



<!-- Auto Generated Below -->


## Properties

| Property             | Attribute | Description                 | Type     | Default     |
| -------------------- | --------- | --------------------------- | -------- | ----------- |
| `items` _(required)_ | `items`   | The list of items selected. | `Item[]` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-actions-bar](../../dnn-rm-actions-bar)
 - [dnn-rm-folder-context-menu](../../context-menus/dnn-rm-folder-context-menu)

### Depends on

- dnn-modal
- [dnn-rm-unlink-items](../../dnn-rm-unlink-items)

### Graph
```mermaid
graph TD;
  dnn-action-unlink-items --> dnn-modal
  dnn-action-unlink-items --> dnn-rm-unlink-items
  dnn-rm-unlink-items --> dnn-rm-progress-bar
  dnn-rm-unlink-items --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-actions-bar --> dnn-action-unlink-items
  dnn-rm-folder-context-menu --> dnn-action-unlink-items
  style dnn-action-unlink-items fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
