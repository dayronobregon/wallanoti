/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UserDetailsResponse } from '../models/UserDetailsResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class UserService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @returns UserDetailsResponse OK
     * @throws ApiError
     */
    public getUser(): CancelablePromise<UserDetailsResponse> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/User',
        });
    }
}
