#pragma once
#include "com.unity.xr.arcore.h"

// Must match UnityEngine.XR.ARSubsystems.XRResultStatus
struct UXRResultStatus
{
public:
    typedef enum UXRStatusCode
    {
        kStatusPlatformQualifiedSuccess = 1,
        kStatusUnqualifiedSuccess = 0,
        kStatusPlatformError = -1,
        kStatusUnknownError = -2,
        kStatusProviderUninitialized = -3,
        kStatusProviderNotStarted = -4,
        kStatusValidationFailure = -5,
    } UXRStatusCode;

    UXRStatusCode statusCode;
    int nativeStatusCode;

    bool IsError()
    {
        return statusCode < 0;
    }

    bool IsSuccess()
    {
        return statusCode >= 0;
    }
};

class UXRResultStatusUtils
{
public:
    static UXRResultStatus Create(UXRResultStatus::UXRStatusCode statusCode, int nativeStatusCode)
    {
        return UXRResultStatus{ statusCode, nativeStatusCode };
    }

    static UXRResultStatus Create(UXRResultStatus::UXRStatusCode statusCode)
    {
        return UXRResultStatus{ statusCode, 0 };
    }

    static UXRResultStatus Create(ArCloudAnchorState nativeStatusCode)
    {
        UXRResultStatus status;
        if (nativeStatusCode == AR_CLOUD_ANCHOR_STATE_SUCCESS)
        {
            status.statusCode = UXRResultStatus::kStatusUnqualifiedSuccess;
        }
        else if (nativeStatusCode == AR_CLOUD_ANCHOR_STATE_NONE)
        {
            status.statusCode = UXRResultStatus::kStatusPlatformQualifiedSuccess;
        }
        else
        {
            status.statusCode = UXRResultStatus::kStatusPlatformError;
        }

        status.nativeStatusCode = nativeStatusCode;
        return status;
    }
};
