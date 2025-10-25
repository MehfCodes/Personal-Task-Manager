using System;
using PTM.Contracts.Requests;
using PTM.Contracts.Requests.User;
using PTM.Contracts.Response.User;

namespace PTM.Application.Interfaces.Services;

public interface IUserPasswordService
{
    Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request);
    Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request);
}
