export interface Criterion {
  id: number;
  name: string;
  areaKeywords: string;
  requirementKeywords: string;
  minRequirementMatches: number;
  isActive: boolean;
}

export interface Broker {
  id: number;
  name: string;
  websiteUrl: string;
  assignmentPortalUrl: string | null;
  loginUrl: string | null;
  connectorKey: string;
  selectorConfigJson: string | null;
  isEnabled: boolean;
  lastScrapedAt: string | null;
  lastScanSucceeded: boolean | null;
  lastScanError: string | null;
  assignmentCount: number;
  hasLoginCredentials: boolean;
  loginUsername: string | null;
}

export interface UpsertBroker {
  name: string;
  websiteUrl: string;
  assignmentPortalUrl: string | null;
  loginUrl: string | null;
  connectorKey: string;
  selectorConfigJson: string | null;
  isEnabled: boolean;
  loginUsername: string | null;
  loginPassword: string | null;
}

export interface AssignmentItem {
  id: number;
  title: string;
  description: string | null;
  area: string | null;
  skills: string | null;
  url: string;
  brokerName: string;
  firstSeenAt: string;
  isInteresting: boolean;
}

export interface AssignmentsPage {
  items: AssignmentItem[];
  totalCount: number;
}

export interface MatchItem {
  id: number;
  assignmentId: number;
  title: string;
  url: string;
  area: string | null;
  skills: string | null;
  brokerName: string;
  matchedAt: string;
  isSeen: boolean;
}

export interface MatchesPage {
  items: MatchItem[];
  totalCount: number;
}

export interface AdminUser {
  id: string;
  email: string;
  isApproved: boolean;
}
