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
  connectorKey: string;
  isEnabled: boolean;
  lastScrapedAt: string | null;
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
