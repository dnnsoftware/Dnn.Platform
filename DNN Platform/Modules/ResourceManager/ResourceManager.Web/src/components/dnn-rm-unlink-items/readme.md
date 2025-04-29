# dnn-rm-edit-file

<!-- Auto Generated Below -->

## Properties

| Property             | Attribute | Description                  | Type     | Default     |
| -------------------- | --------- | ---------------------------- | -------- | ----------- |
| `items` _(required)_ | --        | The list of items to delete. | `Item[]` | `undefined` |

## Events

| Event                 | Description                                                                                                        | Type                |
| --------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------- |
| `dnnRmFoldersChanged` | Fires when there is a possibility that some folders have changed. Can be used to force parts of the UI to refresh. | `CustomEvent<void>` |

## Dependencies

### Used by

- [dnn-action-unlink-items](../actions/dnn-action-unlink-items)

### Depends on

- [dnn-rm-progress-bar](../dnn-rm-progress-bar)
- dnn-button

### Graph

```mermaid
graph TD;
  dnn-rm-unlink-items --> dnn-rm-progress-bar
  dnn-rm-unlink-items --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-unlink-items --> dnn-rm-unlink-items
  style dnn-rm-unlink-items fill:#f9f,stroke:#333,stroke-width:4px
```

---

_Built with [StencilJS](https://stenciljs.com/)_
