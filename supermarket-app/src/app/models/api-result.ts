export interface ApiResult {
    success:boolean,
    message:string
}

export interface ApiResultData<T> extends ApiResult{
    item: T
}

export interface ApiResultPagination<T> extends ApiResult{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}


