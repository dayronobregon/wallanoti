/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { VerifyRequest } from '../models/VerifyRequest';
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
     * @param requestBody
     * @returns string OK
     * @throws ApiError
     */
    public postAuthVerify(
        requestBody?: VerifyRequest,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'POST',
            url: '/Auth/verify',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
