/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { User } from '../models/User';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class UserService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @returns User OK
     * @throws ApiError
     */
    public getUser(): CancelablePromise<User> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/User',
        });
    }
}
