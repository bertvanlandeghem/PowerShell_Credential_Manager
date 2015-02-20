﻿using PSCredentialManager.CredentialManagerApi.Enums;
using PSCredentialManager.CredentialManagerApi.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PSCredentialManager.CredentialManagerApi.Support
{
    sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        // Set the handle.
        internal CriticalCredentialHandle(IntPtr preexistingHandle)
        {
            SetHandle(preexistingHandle);
        }

        internal Credential GetCredential()
        {
            if (!IsInvalid)
            {
                // Get the Credential from the mem location
                NativeCredential ncred = (NativeCredential)Marshal.PtrToStructure(handle,
                      typeof(NativeCredential));

                // Create a managed Credential type and fill it with data from the native counterpart.
                Credential cred = new Credential();
                cred.CredentialBlobSize = ncred.CredentialBlobSize;
                cred.CredentialBlob = Marshal.PtrToStringUni(ncred.CredentialBlob,
       (int)ncred.CredentialBlobSize / 2);
                cred.UserName = Marshal.PtrToStringUni(ncred.UserName);
                cred.TargetName = Marshal.PtrToStringUni(ncred.TargetName);
                cred.TargetAlias = Marshal.PtrToStringUni(ncred.TargetAlias);
                cred.Type = ncred.Type;
                cred.Flags = ncred.Flags;
                cred.Persist = (CRED_PERSIST)ncred.Persist;
                return cred;
            }
            else
            {
                throw new InvalidOperationException("Invalid CriticalHandle!");
            }
        }

        // Perform any specific actions to release the handle in the ReleaseHandle method.
        // Often, you need to use Pinvoke to make a call into the Win32 API to release the 
        // handle. In this case, however, we can use the Marshal class to release the unmanaged memory.

        override protected bool ReleaseHandle()
        {
            // If the handle was set, free it. Return success.
            if (!IsInvalid)
            {
                // NOTE: We should also ZERO out the memory allocated to the handle, before free'ing it
                // so there are no traces of the sensitive data left in memory.
                Imports.CredFree(handle);
                // Mark the handle as invalid for future users.
                SetHandleAsInvalid();
                return true;
            }
            // Return false. 
            return false;
        }
    }
}