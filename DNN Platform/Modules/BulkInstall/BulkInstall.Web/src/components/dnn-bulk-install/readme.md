# my-component



<!-- Auto Generated Below -->


## Properties

| Property                | Attribute   | Description | Type     | Default     |
| ----------------------- | ----------- | ----------- | -------- | ----------- |
| `moduleId` _(required)_ | `module-id` |             | `number` | `undefined` |


## Dependencies

### Depends on

- dnn-tabs
- dnn-tab
- [bulk-install-install](../tabs/bulk-install-install)
- [bulk-install-logs](../tabs/bulk-install-logs)
- [bulk-install-api-users](../tabs/bulk-install-api-users)
- [bulk-install-ip-safelist](../tabs/bulk-install-ip-safelist)

### Graph
```mermaid
graph TD;
  dnn-bulk-install --> dnn-tabs
  dnn-bulk-install --> dnn-tab
  dnn-bulk-install --> bulk-install-install
  dnn-bulk-install --> bulk-install-logs
  dnn-bulk-install --> bulk-install-api-users
  dnn-bulk-install --> bulk-install-ip-safelist
  bulk-install-install --> dnn-dropzone
  bulk-install-install --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  bulk-install-api-users --> dnn-button
  bulk-install-api-users --> dnn-modal
  bulk-install-api-users --> dnn-input
  bulk-install-api-users --> dnn-checkbox
  dnn-input --> dnn-fieldset
  bulk-install-ip-safelist --> dnn-input
  bulk-install-ip-safelist --> dnn-button
  bulk-install-ip-safelist --> dnn-toggle
  style dnn-bulk-install fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
