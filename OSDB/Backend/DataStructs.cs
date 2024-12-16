using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OSDB.Backend
{
    public class SearchSubtitlesRequest
    {
        public string SubLanguageId = string.Empty;
        public string MovieHash = string.Empty;
        public string MovieByteSize = string.Empty;
        public string ImdbId = string.Empty;
        public string Query = string.Empty;
        public int? Season = null;
        public int? Episode = null;
    }

    public class SearchSubtitlesInfo
    {
        public string SubFileName;
        public string SubHash;
        public string IDSubtitle;
        public string SubLanguageID;
        public string SubBad;
        public string SubRating;
        public string IDMovie;
        public string IDMovieImdb;
        public string MovieName;
        public string MovieNameEng;
        public string MovieYear;
        public string LanguageName;
        public string SubDownloadLink;
        public String ISO639;
        public string SubtitlesLink;
    }

    [XmlRoot("methodResponse", Namespace = "")]
    public class Response
    {
        [XmlElement("params", Namespace = "")] public List<ResponseParams> Params { get; set; }
    }

    public class ResponseParams
    {
        [XmlElement("param", Namespace = "")] public ResponseParam Param { get; set; }
    }

    public class ResponseParam
    {
        [XmlElement("value", Namespace = "")] public ResponseValue Value { get; set; }
    }

    public class ResponseValue
    {
        [XmlArray("struct", Namespace = ""), XmlArrayItem("member", Namespace = "")]
        public List<Member> Member { get; set; }
    }

    public class Member
    {
        [XmlElement("name", Namespace = "")] public string Name { get; set; }

        [XmlElement("value", Namespace = "")] public SubMember Value { get; set; }
    }

    public class SubMember
    {
        [XmlElement("array", Namespace = "")] public SubMemberValue Value { get; set; }
    }

    public class MemberValue
    {
        [XmlElement("string", Namespace = "")] public string Value { get; set; }
    }

    public class SubMemberValue
    {
        [XmlArray("data", Namespace = ""), XmlArrayItem("value", Namespace = "")]
        public List<InnerSubValue> Value { get; set; }
    }

    public class InnerSubValue
    {
        [XmlArray("struct", Namespace = ""), XmlArrayItem("member", Namespace = "")]
        public List<LastMember> Value { get; set; }
    }

    public class LastMember
    {
        [XmlElement("name", Namespace = "")] public string Name { get; set; }

        [XmlElement("value", Namespace = "")] public MemberValue Value { get; set; }
    }
}