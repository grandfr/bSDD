﻿using System.Linq;
using RestSharp;
using bSDD.NET.Model.Objects;
using System.Collections.Generic;
using System.Net;
using System;

namespace bSDD.NET
{
    public class Bsdd
    {
        private string Version = "4.0";

        public string BaseUrl { get; }

        private RestClient restclient;
        private IfdAPISession Session;
        private IfdLanguageList Languages;

        public Bsdd(string url, string email, string password)
        {
            BaseUrl = $"{url}/api/{Version}";
            restclient = new RestClient(BaseUrl);
            Session = Login(email, password);
            GetLanguages();
        }

        private IfdAPISession Login(string email, string password)
            {
            var request = new RestRequest("/session/login", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("email", email); 
            request.AddParameter("password", password); 

            var response = restclient.Execute<IfdAPISession>(request);
            return response.Data;
        }

        private void GetLanguages()
        {
            var request = new RestRequest("/IfdLanguage", Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdLanguageList>(request);
            Languages = response.Data;
        }

        public IfdNameList SearchNames(string searchstring)
        {
            var request = new RestRequest("/IfdName/search/" + searchstring, Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdNameList>(request);     
            return response.Data;
        }

        public IfdNameList SearchNamesByLanguage(string searchstring, string language)
        {
            string languageguid = Languages.IfdLanguage.Where(x => x.LanguageCode == language).FirstOrDefault().Guid;
            var request = new RestRequest("/IfdName/search/filter/language/" + languageguid + "/" + searchstring, Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdNameList>(request);
            return response.Data;
        }

        public IfdConcept GetConcept(string ifdGuid)
        {
            var request = new RestRequest($"/IfdConcept/{ifdGuid}", Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdConcept>(request);
            return response.Data;
        }

        public List<IfdConceptInRelationship> GetTranslationOfConcept(string ifdGuid, string language)
        {
            var languageGuid = Languages.IfdLanguage.Where(x => x.LanguageCode.ToLower() == language.ToLower()).FirstOrDefault().Guid;
            var request = new RestRequest($"/IfdConcept/{ifdGuid}/children/filter/language/{languageGuid}", Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<List<IfdConceptInRelationship>>(request);
            return response.Data;
        }

        public IfdConceptList SearchConcepts(string searchstring)
        {
            var request = new RestRequest("/IfdConcept/search/" + searchstring, Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdConceptList>(request);
            return response.Data;
        }

        public IfdConceptList SearchNests(string searchString)
        {
            var request = new RestRequest($"/IfdConcept/search/filter/type/NEST/{searchString}", Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdConceptList>(request);
            return response.Data;
        }

        public IfdConceptList SearchProperties(string searchString)
        {
            var request = new RestRequest($"/IfdConcept/search/filter/type/PROPERTY/{searchString}", Method.GET);
            request.AddHeader("Accept", "application/json");
            var response = restclient.Execute<IfdConceptList>(request);
            return response.Data;
        }

        public void UpdateConceptStatus(string conceptGuid, IfdStatusEnum newStatus)
        {
            //PUT /IfdConcept/{guid}/status
            //Updates the status of the given concept. You need minimum IFD_EDITOR access to use this method.

            var request = new RestRequest($"/IfdConcept/{conceptGuid}/status", Method.PUT);
            request.AddHeader("Accept", "application/json");
            request.AddCookie("peregrineapisessionid", Session.Guid);
            request.AddParameter("status", newStatus.ToString(), ParameterType.GetOrPost);

            var response = restclient.Execute(request);
        }

        public IfdBase InsertConceptName(string conceptGuid, string languageCode, string name)
        {
            //POST /IfdConcept/{guid}/name
            //Adds a new name to the given concept in the given language. You need minimum IFD_EDITOR access to use this method.

            var request = new RestRequest($"/IfdConcept/{conceptGuid}/name", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddCookie("peregrineapisessionid", Session.Guid);
            request.AddParameter("languageGuid", Languages.IfdLanguage.Where(x => x.LanguageCode.ToLower() == languageCode.ToLower()).FirstOrDefault().Guid, ParameterType.GetOrPost);
            request.AddParameter("name", name, ParameterType.GetOrPost);
            request.AddParameter("nameType", IfdNameTypeEnum.FULLNAME, ParameterType.GetOrPost);

            var response = restclient.Execute<IfdBase>(request);
            return response.Data;
        }

        public IfdBase UpdateConceptName(string conceptGuid, string nameGuid, string languageCode, string name)
        {
            //DELETE /IfdConcept/{guid}/name/{nameGuid}
            //POST /IfdConcept/{guid}/name
            //Delets and re-adds the name of the concept in the given language

            var request = new RestRequest($"/IfdConcept/{conceptGuid}/description/{nameGuid}", Method.DELETE);
            request.AddHeader("Accept", "application/json");
            request.AddCookie("peregrineapisessionid", Session.Guid);

            var responseDelete = restclient.Execute<IfdBase>(request);

            var responseInsert = InsertConceptName(conceptGuid, languageCode, name);
            return responseInsert;
        }

        public IfdBase InsertConceptDefinition(string conceptGuid, string languageCode, string definition)
        {
            //POST api/4.0/IfdConcept/{guid}/definition
            //Adds a new description to the given concept in the given language. You need minimum IFD_EDITOR access to use this method.

            var request = new RestRequest($"/IfdConcept/{conceptGuid}/definition", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddCookie("peregrineapisessionid", Session.Guid);
            request.AddParameter("languageGuid", Languages.IfdLanguage.Where(x => x.LanguageCode.ToLower() == languageCode.ToLower()).FirstOrDefault().Guid, ParameterType.GetOrPost);
            request.AddParameter("definition", definition, ParameterType.GetOrPost);

            var response = restclient.Execute<IfdBase>(request);
            return response.Data;
        }

        public IfdBase UpdateConceptDefinition(string conceptGuid, string descriptionGuid, string languageCode, string definition)
        {
            //DELETE /IfdConcept/{guid}/description/{descriptionGuid}
            //POST /IfdConcept/{guid}/definition
            //Delets and re-adds the description of the concept in the given language

            var request = new RestRequest($"/IfdConcept/{conceptGuid}/description/{descriptionGuid}", Method.DELETE);
            request.AddHeader("Accept", "application/json");
            request.AddCookie("peregrineapisessionid", Session.Guid);

            var responseDelete = restclient.Execute<IfdBase>(request);

            var responseInsert = InsertConceptDefinition(conceptGuid, languageCode, definition);
            return responseInsert;
        }
    }
}