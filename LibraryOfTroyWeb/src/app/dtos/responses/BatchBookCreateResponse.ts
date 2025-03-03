export interface BatchBookCreateResponse {
  totalRequested: number;
  successCount: number;
  failedCount: number;
  errors: string[];
}
