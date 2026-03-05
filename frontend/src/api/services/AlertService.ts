/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AlertCounter } from '../models/AlertCounter';
import type { GetAlertsByUserIdResponse } from '../models/GetAlertsByUserIdResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class AlertService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @returns GetAlertsByUserIdResponse OK
     * @throws ApiError
     */
    public getAlert(): CancelablePromise<Array<GetAlertsByUserIdResponse>> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/Alert',
            errors: {
                401: `Unauthorized`,
            },
        });
    }
    /**
     * @param alertName
     * @param url
     * @returns string Created
     * @throws ApiError
     */
    public postAlert(
        alertName: string,
        url: string,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'POST',
            url: '/Alert',
            query: {
                'alertName': alertName,
                'url': url,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }
    /**
     * @param alertId
     * @returns string Accepted
     * @throws ApiError
     */
    public deleteAlert(
        alertId: string,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'DELETE',
            url: '/Alert/{alertId}',
            path: {
                'alertId': alertId,
            },
            errors: {
                401: `Unauthorized`,
            },
        });
    }
    /**
     * @param alertId
     * @returns any Accepted
     * @throws ApiError
     */
    public patchAlertDeactivate(
        alertId: string,
    ): CancelablePromise<any> {
        return this.httpRequest.request({
            method: 'PATCH',
            url: '/Alert/{alertId}/deactivate',
            path: {
                'alertId': alertId,
            },
            errors: {
                401: `Unauthorized`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @param alertId
     * @returns any Accepted
     * @throws ApiError
     */
    public patchAlertActivate(
        alertId: string,
    ): CancelablePromise<any> {
        return this.httpRequest.request({
            method: 'PATCH',
            url: '/Alert/{alertId}/activate',
            path: {
                'alertId': alertId,
            },
            errors: {
                401: `Unauthorized`,
                404: `Not Found`,
            },
        });
    }
    /**
     * @param alertId
     * @returns AlertCounter OK
     * @throws ApiError
     */
    public getAlertCounter(
        alertId: string,
    ): CancelablePromise<AlertCounter> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/Alert/{alertId}/counter',
            path: {
                'alertId': alertId,
            },
            errors: {
                401: `Unauthorized`,
                404: `Not Found`,
            },
        });
    }
}
