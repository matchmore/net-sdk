﻿/* 
 * MATCHMORE ALPS Core REST API
 *
 * ## ALPS by [MATCHMORE](https://matchmore.io)  The first version of the MATCHMORE API is an exciting step to allow developers use a context-aware pub/sub cloud service.  A lot of mobile applications and their use cases may be modeled using this approach and can therefore profit from using MATCHMORE as their backend service.  **Build something great with [ALPS by MATCHMORE](https://matchmore.io)!**   Once you've [registered your client](https://matchmore.io/account/register/) it's easy start using our awesome cloud based context-aware pub/sub (admitted, a lot of buzzwords).  ## RESTful API We do our best to have all our URLs be [RESTful](https://en.wikipedia.org/wiki/Representational_state_transfer). Every endpoint (URL) may support one of four different http verbs. GET requests fetch information about an object, POST requests create objects, PUT requests update objects, and finally DELETE requests will delete objects.  ## Domain Model  This is the current domain model extended by an ontology of devices and separation between the developer portal and the ALPS Core.      +- -- -- -- -- --+    +- -- -- -- -- -- --+     | Developer +- -- -+ Application |     +- -- -- -- -- --+    +- -- -- -+- -- -- -+                             |                        \"Developer Portal\"     ........................+..........................................                             |                        \"ALPS Core\"                         +- --+- --+                         | World |                         +- --+- --+                             |                           +- -- -- -- -- -- --+                             |                     +- -- --+ Publication |                             |                     |     +- -- -- -+- -- -- -+                             |                     |            |                             |                     |            |                             |                     |            |                             |                     |        +- --+- --+                        +- -- -+- --+- -- -- -- -- -- -- -- --+        | Match |                        | Device |                          +- --+- --+                        +- -- -+- --+- -- -- -- -- -- -- -- --+            |                             |                     |            |                             |                     |            |                             |                     |     +- -- -- -+- -- -- --+             +- -- -- -- -- -- -- --+- -- -- -- -- -- -- -+      +- -- --+ Subscription |             |               |              |            +- -- -- -- -- -- -- -+        +- -- -+- --+      +- -- -+- -- -+    +- -- -+- --+        |   Pin  |      | iBeacon |    | Mobile |        +- -- -+- --+      +- -- -- -- --+    +- -- -+- --+             |                              |             |         +- -- -- -- -- -+         |             +- -- -- -- --+ Location +- -- -- -- --+                       +- -- -- -- -- -+  1.  A **developer** is a mobile application developer registered in the     developer portal and allowed to use the **ALPS Developer Portal**.  A     developer might register one or more **applications** to use the     **ALPS Core cloud service**.  For developer/application pair a new     **world** is created in the **ALPS Core** and assigned an **API key** to     enable access to the ALPS Core cloud service **RESTful API**.  During     the registration, the developer needs to provide additional     configuration information for each application, e.g. its default     **push endpoint** URI for match delivery, etc. 2.  A [**device**](#tag/device) might be either *virtual* like a **pin device** or     *physical* like a **mobile device** or **iBeacon device**.  A [**pin     device**](#tag/device) is one that has geographical [**location**](#tag/location) associated with it     but is not represented by any object in the physical world; usually     it's location doesn't change frequently if at all.  A [**mobile     device**](#tag/device) is one that potentially moves together with its user and     therefore has a geographical location associated with it.  A mobile     device is typically a location-aware smartphone, which knows its     location thanks to GPS or to some other means like cell tower     triangulation, etc.  An [**iBeacon device**](#tag/device) represents an Apple     conform [iBeacon](https://developer.apple.com/ibeacon/) announcing its presence via Bluetooth LE     advertising packets which can be detected by a other mobile device.     It doesn't necessary has any location associated with it but it     serves to detect and announce its proximity to other **mobile     devices**. 3.  The hardware and software stack running on a given device is known     as its **platform**.  This include its hardware-related capabilities,     its operating systems, as well as the set of libraries (APIs)     offered to developers in order to program it. 4.  A devices may issue publications and subscriptions     at **any time**; it may also cancel publications and subscriptions     issued previously.  **Publications** and **subscriptions** do have a     definable, finite duration, after which they are deleted from the     ALPS Core cloud service and don't participate anymore in the     matching process. 5.  A [**publication**](#tag/publication) is similar to a Java Messaging Service (JMS)     publication extended with the notion of a **geographical zone**.  The     zone is defined as **circle** with a center at the given location and     a range around that location. 6.  A [**subscription**](#tag/subscription) is similar to a JMS subscription extended with the     notion of **geographical zone**. Again, the zone being defined as     **circle** with a center at the given location and a range around     that location. 7.  **Publications** and **subscriptions** which are associated with a     **mobile device**, e.g. user's mobile phone, potentially **follow the     movements** of the user carrying the device and therefore change     their associated location. 8.  A [**match**](#tag/match) between a publication and a subscription occurs when both     of the following two conditions hold:     1.  There is a **context match** occurs when for instance the         subscription zone overlaps with the publication zone or a         **proximity event** with an iBeacon device within the defined         range occurred.     2.  There is a **content match**: the publication and the subscription         match with respect to their JMS counterparts, i.e., they were         issued on the same topic and have compatible properties and the         evaluation of the selector against those properties returns true         value. 9.  A **push notification** is an asynchronous mechanism that allows an     application to receive matches for a subscription on his/her device.     Such a mechanism is clearly dependent on the device’s platform and     capabilities.  In order to use push notifications, an application must     first register a device (and possibly an application on that     device) with the ALPS core cloud service. 10. Whenever a **match** between a publication and a subscription     occurs, the device which owns the subscription receives that match     *asynchronously* via a push notification if there exists a     registered **push endpoint**.  A **push endpoint** is an URI which is     able to consume the matches for a particular device and     subscription.  The **push endpoint** doesn't necessary point to a     **mobile device** but is rather a very flexible mechanism to define     where the matches should be delivered. 11. Matches can also be retrieved by issuing a API call for a     particular device.   <a id=\"orgae4fb18\"></a>  ## Device Types                     +- -- -+- --+                    | Device |                    +- -- -- -- -+                    | id     |                    | name   |                    | group  |                    +- -- -+- --+                         |         +- -- -- -- -- -- -- --+- -- -- -- -- -- -- -- -+         |               |                |     +- --+- --+   +- -- -- --+- -- -- -+    +- -- -+- -- --+     |  Pin  |   | iBeacon      |    | Mobile   |     +- --+- --+   +- -- -- -- -- -- -- -+    +- -- -- -- -- -+         |       | proximityUUID|    | platform |         |       | major        |    | token    |         |       | minor        |    +- -- -+- -- --+         |       +- -- -- --+- -- -- -+         |         |               |                |         |               | <- -???         |         |          +- -- -+- -- --+          |         +- -- -- -- -- -+ Location +- -- -- -- -- -+                    +- -- -- -- -- -+   <a id=\"org68cc0d8\"></a>  ### Generic `Device`  -   id -   name -   group  <a id=\"orgc430925\"></a>  ### `PinDevice`  -   location   <a id=\"orgecaed9f\"></a>  ### `iBeaconDevice`  -   proximityUUID -   major -   minor   <a id=\"org7b09b62\"></a>  ### `MobileDevice`  -   platform -   deviceToken -   location 
 *
 * OpenAPI spec version: 0.5.0
 * Contact: support@matchmore.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Matchmore.Model
{
    /// <summary>
    /// Publications
    /// </summary>
    [DataContract]
    public partial class Publications : List<Publication>,  IEquatable<Publications>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Publications" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        public Publications() : base()
        {
        }
        
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Publications {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as Publications);
        }

        /// <summary>
        /// Returns true if Publications instances are equal
        /// </summary>
        /// <param name="input">Instance of Publications to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Publications input)
        {
            if (input == null)
                return false;

            return base.Equals(input);
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = base.GetHashCode();
                return hashCode;
            }
        }
    }

}
