export interface BookBatchCreateResponse {
  totalRequested: number;
  successCount: number;
  failedCount: number;
  errors: string[];
}
