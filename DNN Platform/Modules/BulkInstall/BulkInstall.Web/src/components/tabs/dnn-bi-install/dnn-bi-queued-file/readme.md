# bulk-install-queued-file



<!-- Auto Generated Below -->


## Properties

| Property                         | Attribute              | Description                          | Type      | Default     |
| -------------------------------- | ---------------------- | ------------------------------------ | --------- | ----------- |
| `file` _(required)_              | --                     | The file to upload.                  | `File`    | `undefined` |
| `maxUploadFileSize` _(required)_ | `max-upload-file-size` | The maximal allowed file upload size | `number`  | `undefined` |
| `session` _(required)_           | --                     | The current session.                 | `Session` | `undefined` |


## Events

| Event             | Description | Type                                                                                                           |
| ----------------- | ----------- | -------------------------------------------------------------------------------------------------------------- |
| `uploadCompleted` |             | `CustomEvent<UploadStatus.Cancelled \| UploadStatus.Error \| UploadStatus.InProgress \| UploadStatus.Success>` |


## Dependencies

### Used by

 - [dnn-bi-install](..)

### Depends on

- [dnn-bi-dismiss-icon](../../../icons/dnn-bi-dismiss-icon)
- [dnn-bi-checkmark-icon](../../../icons/dnn-bi-checkmark-icon)

### Graph
```mermaid
graph TD;
  dnn-bi-queued-file --> dnn-bi-dismiss-icon
  dnn-bi-queued-file --> dnn-bi-checkmark-icon
  dnn-bi-install --> dnn-bi-queued-file
  style dnn-bi-queued-file fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
