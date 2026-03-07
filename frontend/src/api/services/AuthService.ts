/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LoginRequest } from '../models/LoginRequest';
import type { VerifyRequest } from '../models/VerifyRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class AuthService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @param requestBody
     * @returns string OK
     * @throws ApiError
     */
    public postAuthLogin(
        requestBody: LoginRequest,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'POST',
            url: '/Auth/login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param requestBody
     * @returns string OK
     * @throws ApiError
     */
    public postAuthVerify(
        requestBody: VerifyRequest,
    ): CancelablePromise<string> {
        return this.httpRequest.request({
            method: 'POST',
            url: '/Auth/verify',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
