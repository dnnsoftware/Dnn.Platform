# dnn-rm-edit-folder



<!-- Auto Generated Below -->


## Events

| Event                 | Description                                                                                                        | Type                |
| --------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------- |
| `dnnRmFoldersChanged` | Fires when there is a possibility that some folders have changed. Can be used to force parts of the UI to refresh. | `CustomEvent<void>` |


## Dependencies

### Used by

 - [dnn-action-create-folder](../actions/dnn-action-create-folder)

### Depends on

- dnn-button

### Graph
```mermaid
graph TD;
  dnn-rm-create-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-create-folder --> dnn-rm-create-folder
  style dnn-rm-create-folder fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
