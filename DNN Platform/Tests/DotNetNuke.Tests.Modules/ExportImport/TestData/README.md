# Legacy Site Export/Import test fixtures

These are genuine **LiteDB v3.1.0** database files (on-disk format version 7), the format produced by
DNN 9.10.2-era Site Export/Import (`DotNetNuke.SiteExportImport.Library` 9.10.2). They are used by
`ExportImportRepositoryTests` to reproduce and guard against the site-import failure where
`VerifyImportPackage` returned HTTP 400 `Package is not valid. Technical Details:Detected loop in
FindAll({0})` (eng-maintenance #22222).

LiteDB 5.0.21 (the version DNN 10.x pins) cannot *write* the v3 format, so these fixtures are checked
in as binary rather than generated at test time.

| File | `ExportFolder` rows | Purpose |
| --- | --- | --- |
| `legacy_v3_large.dnndb` | 3,000 | Above the ~2,550 LiteDB rebuild loop-guard threshold. Opening this with the old `Upgrade = true` path throws `Detected loop in FindAll({0})`; the fix must open it and return all rows. |
| `legacy_v3_small.dnndb` | 50 | Below the threshold. Regression check that small legacy packages still open. |

Each `ExportFolder` document has `_id` (1..N), `FolderPath` = `F/<id>`, and `ReferenceId` = `<id> % 10`.
Both fixtures also contain a small `ExportPackage` collection.
