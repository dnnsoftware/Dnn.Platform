# my-component



<!-- Auto Generated Below -->


## Dependencies

### Depends on

- dnn-tabs
- dnn-tab
- [event-log](../tabs/event-log)
- [api-users](../tabs/api-users)
- [ip-safelist](../tabs/ip-safelist)

### Graph
```mermaid
graph TD;
  dnn-bulk-install --> dnn-tabs
  dnn-bulk-install --> dnn-tab
  dnn-bulk-install --> event-log
  dnn-bulk-install --> api-users
  dnn-bulk-install --> ip-safelist
  api-users --> dnn-input
  api-users --> dnn-checkbox
  api-users --> dnn-button
  dnn-input --> dnn-fieldset
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  ip-safelist --> dnn-input
  ip-safelist --> dnn-button
  ip-safelist --> dnn-toggle
  style dnn-bulk-install fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
