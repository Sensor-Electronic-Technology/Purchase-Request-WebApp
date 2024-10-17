using Domain.PurchaseRequests.TypeConstants;

namespace Webapp.Data;

public record UserActionEventArg(PrUserAction Action,string PrId);