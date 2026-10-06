# bulk-install-queued-file



<!-- Auto Generated Below -->


## Properties

| Property           | Attribute | Description      | Type         | Default     |
| ------------------ | --------- | ---------------- | ------------ | ----------- |
| `job` _(required)_ | --        | The install job. | `InstallJob` | `undefined` |


## Dependencies

### Used by

 - [dnn-bi-install](..)

### Depends on

- [dnn-bi-package-job](../dnn-bi-package-job)
- [dnn-bi-clock-icon](../../../icons/dnn-bi-clock-icon)
- [dnn-bi-circle-x-icon](../../../icons/dnn-bi-circle-x-icon)
- [dnn-bi-checkmark-icon](../../../icons/dnn-bi-checkmark-icon)

### Graph
```mermaid
graph TD;
  dnn-bi-install-job --> dnn-bi-package-job
  dnn-bi-install-job --> dnn-bi-clock-icon
  dnn-bi-install-job --> dnn-bi-circle-x-icon
  dnn-bi-install-job --> dnn-bi-checkmark-icon
  dnn-bi-package-job --> dnn-bi-circle-x-icon
  dnn-bi-package-job --> dnn-bi-checkmark-icon
  dnn-bi-install --> dnn-bi-install-job
  style dnn-bi-install-job fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
