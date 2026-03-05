/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { NotificationResponse } from '../models/NotificationResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class NotificationService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @returns NotificationResponse OK
     * @throws ApiError
     */
    public getNotification(): CancelablePromise<Array<NotificationResponse>> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/Notification',
            errors: {
                403: `Forbidden`,
            },
        });
    }
}
