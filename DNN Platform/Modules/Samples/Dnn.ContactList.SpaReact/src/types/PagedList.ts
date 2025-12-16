export interface PagedList<T> {
  Data: T[];
  PageIndex: number;
  PageSize: number;
  TotalCount: number;
  PageCount: number;
  IsFirstPage: boolean;
  IsLastPage: boolean;
}
