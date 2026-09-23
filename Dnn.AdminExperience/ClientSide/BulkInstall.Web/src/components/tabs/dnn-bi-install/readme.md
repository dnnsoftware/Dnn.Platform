# bulk-install-install

<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-bulk-install](../../dnn-bulk-install)

### Depends on

- dnn-dropzone
- [dnn-bi-install-job](dnn-bi-install-job)
- [dnn-bi-queued-file](dnn-bi-queued-file)
- dnn-button

### Graph
```mermaid
graph TD;
  dnn-bi-install --> dnn-dropzone
  dnn-bi-install --> dnn-bi-install-job
  dnn-bi-install --> dnn-bi-queued-file
  dnn-bi-install --> dnn-button
  dnn-bi-install-job --> dnn-bi-package-job
  dnn-bi-install-job --> dnn-bi-clock-icon
  dnn-bi-install-job --> dnn-bi-circle-x-icon
  dnn-bi-install-job --> dnn-bi-checkmark-icon
  dnn-bi-package-job --> dnn-bi-circle-x-icon
  dnn-bi-package-job --> dnn-bi-checkmark-icon
  dnn-bi-queued-file --> dnn-bi-dismiss-icon
  dnn-bi-queued-file --> dnn-bi-checkmark-icon
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-bulk-install --> dnn-bi-install
  style dnn-bi-install fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
