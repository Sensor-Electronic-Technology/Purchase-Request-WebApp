﻿using Domain.PurchaseRequests.TypeConstants;

namespace Domain.PurchaseRequests.Dto;

public record PurchaseRequestStatus(string Id, string Name,PrStatus Status,bool IsComplete,string DateStr,string RequesterId,string ApproverId);
