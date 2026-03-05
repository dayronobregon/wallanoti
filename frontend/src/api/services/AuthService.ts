/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class AuthService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @param userName
     * @returns string OK
     * @throws ApiError
     */
    public getAuthLogin(
        userName: string,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/Auth/login/{userName}',
            path: {
                'userName': userName,
            },
        });
    }
    /**
     * @param userName
     * @param code
     * @returns string OK
     * @throws ApiError
     */
    public getAuthVerify(
        userName: string,
        code: string,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/Auth/verify/{userName}/{code}',
            path: {
                'userName': userName,
                'code': code,
            },
        });
    }
}
