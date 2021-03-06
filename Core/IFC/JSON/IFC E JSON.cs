// MIT License
// Copyright (c) 2016 Geometry Gym Pty Ltd

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace GeometryGym.Ifc
{
	public partial class IfcEdge : IfcTopologicalRepresentationItem //SUPERTYPE OF(ONEOF(IfcEdgeCurve, IfcOrientedEdge, IfcSubedge))
	{
		//internal int mEdgeStart, mEdgeEnd;// : IfcVertex;
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			JObject jobj = obj.GetValue("EdgeStart", StringComparison.InvariantCultureIgnoreCase) as JObject;
			if (jobj != null)
				EdgeStart = mDatabase.parseJObject<IfcVertex>(jobj);
			jobj = obj.GetValue("EdgeEnd", StringComparison.InvariantCultureIgnoreCase) as JObject;
			if (jobj != null)
				EdgeEnd = mDatabase.parseJObject<IfcVertex>(jobj);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			obj["EdgeStart"] = mDatabase[mEdgeStart].getJson(this, processed);
			obj["EdgeEnd"] = mDatabase[mEdgeEnd].getJson(this, processed);
		}
	}
	public abstract partial class IfcElement : IfcProduct, IfcStructuralActivityAssignmentSelect //ABSTRACT SUPERTYPE OF (ONEOF(IfcBuildingElement,IfcCivilElement
	{ //,IfcDistributionElement,IfcElementAssembly,IfcElementComponent,IfcFeatureElement,IfcFurnishingElement,IfcGeographicElement,IfcTransportElement ,IfcVirtualElement,IfcElectricalElement SS,IfcEquipmentElement SS)) 
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			JToken token = obj.GetValue("Tag", StringComparison.InvariantCultureIgnoreCase);
			if (token != null)
				Tag = token.Value<string>();
			foreach (IfcRelConnectsStructuralActivity rcsa in mDatabase.extractJArray<IfcRelConnectsStructuralActivity>(obj.GetValue("AssignedStructuralActivity", StringComparison.InvariantCultureIgnoreCase) as JArray))
				rcsa.RelatingElement = this;
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			string tag = Tag;
			if (!string.IsNullOrEmpty(tag))
				obj["Tag"] = tag;
		}
	}
	public partial class IfcElementQuantity : IfcQuantitySet
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			JToken token = obj.GetValue("MethodOfMeasurement", StringComparison.InvariantCultureIgnoreCase);
			if (token != null)
				MethodOfMeasurement = token.Value<string>();
			Quantities = mDatabase.extractJArray<IfcPhysicalQuantity>(obj.GetValue("Quantities", StringComparison.InvariantCultureIgnoreCase) as JArray);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			base.setAttribute(obj, "MethodOfMeasurement", MethodOfMeasurement);
			obj["Quantities"] = new JArray(Quantities.ConvertAll(x => x.getJson(this, processed)));
		}
	}
	public abstract partial class IfcElementarySurface : IfcSurface //	ABSTRACT SUPERTYPE OF(ONEOF(IfcCylindricalSurface, IfcPlane))
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			Position = extractObject<IfcAxis2Placement3D>(obj.GetValue("Position", StringComparison.InvariantCultureIgnoreCase) as JObject);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			obj["Position"] = Position.getJson(this, processed);
		}
	}
	public abstract partial class IfcElementType : IfcTypeProduct //ABSTRACT SUPERTYPE OF(ONEOF(IfcBuildingElementType, IfcDistributionElementType, IfcElementAssemblyType, IfcElementComponentType, IfcFurnishingElementType, IfcGeographicElementType, IfcTransportElementType))
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			JToken token = obj.GetValue("ElementType", StringComparison.InvariantCultureIgnoreCase);
			if (token != null)
				mElementType = token.Value<string>();
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			setAttribute(obj, "ElementType", ElementType);
		}
	}
	public abstract partial class IfcExtendedProperties : IfcPropertyAbstraction //IFC4 ABSTRACT SUPERTYPE OF (ONEOF (IfcMaterialProperties,IfcProfileProperties))
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);

			JToken token = obj.GetValue("Name", StringComparison.InvariantCultureIgnoreCase);
			if (token != null)
				Name = token.Value<string>();
			token = obj.GetValue("Description", StringComparison.InvariantCultureIgnoreCase);
			if (token != null)
				Description = token.Value<string>();
			Properties = mDatabase.extractJArray<IfcProperty>(obj.GetValue("Properties", StringComparison.InvariantCultureIgnoreCase) as JArray);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			if(mDatabase.Release != ReleaseVersion.IFC2x3)
				base.setAttribute(obj, "Name", Name);
			base.setAttribute(obj, "Description", Description);
			if(mProperties.Count > 0)
			{
				JArray array = new JArray();
				foreach (int i in mProperties)
					array.Add(mDatabase[i].getJson(this, processed));
				obj["Properties"] = array;
			}
		}
	}
	public abstract partial class IfcExternalReference : BaseClassIfc, IfcLightDistributionDataSourceSelect, IfcResourceObjectSelect//ABSTRACT SUPERTYPE OF (ONEOF (IfcClassificationReference ,IfcDocumentReference ,IfcExternallyDefinedHatchStyle
	{ //,IfcExternallyDefinedSurfaceStyle ,IfcExternallyDefinedSymbol ,IfcExternallyDefinedTextFont ,IfcLibraryReference)); 
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			Location = extractString(obj.GetValue("Location", StringComparison.InvariantCultureIgnoreCase));
			Identification = extractString(obj.GetValue("Identification", StringComparison.InvariantCultureIgnoreCase));
			Name = extractString(obj.GetValue("Name", StringComparison.InvariantCultureIgnoreCase));
			foreach (IfcExternalReferenceRelationship r in mDatabase.extractJArray<IfcExternalReferenceRelationship>(obj.GetValue("HasExternalReferences", StringComparison.InvariantCultureIgnoreCase) as JArray))
				r.addRelated(this);
			foreach (IfcResourceConstraintRelationship r in mDatabase.extractJArray<IfcResourceConstraintRelationship>(obj.GetValue("HasConstraintRelationships", StringComparison.InvariantCultureIgnoreCase) as JArray))
				r.addRelated(this);
			foreach (IfcExternalReferenceRelationship r in mDatabase.extractJArray<IfcExternalReferenceRelationship>(obj.GetValue("ExternalReferenceForResources", StringComparison.InvariantCultureIgnoreCase) as JArray))
				r.addRelated(this);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host, HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			setAttribute(obj, "Location", Location);
			setAttribute(obj, "Identification", Identification);
			setAttribute(obj, "Name", Name);
			createArray(obj, "HasExternalReferences", HasExternalReferences, this, processed);
			createArray(obj, "HasConstraintRelationships", HasConstraintRelationships, this, processed);
			createArray(obj, "ExternalReferenceForResources", ExternalReferenceForResources, this, processed);
		}
	}

	public partial class IfcExternalReferenceRelationship : IfcResourceLevelRelationship //IFC4
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			RelatingReference = mDatabase.parseJObject<IfcExternalReference>(obj.GetValue("RelatingReference", StringComparison.InvariantCultureIgnoreCase) as JObject);
			RelatedResourceObjects = mDatabase.extractJArray<IfcResourceObjectSelect>(obj.GetValue("RelatedResourceObjects", StringComparison.InvariantCultureIgnoreCase) as JArray);
		}
		protected override void setJSON(JObject obj, BaseClassIfc host,  HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);
			obj["RelatingReference"] = RelatingReference.getJson(this, processed);
		}
	}
	public partial class IfcExtrudedAreaSolid : IfcSweptAreaSolid // SUPERTYPE OF(IfcExtrudedAreaSolidTapered)
	{
		internal override void parseJObject(JObject obj)
		{
			base.parseJObject(obj);
			JObject jobj = obj.GetValue("ExtrudedDirection", StringComparison.InvariantCultureIgnoreCase) as JObject;
			if (jobj != null)
			{
				JToken jtoken = jobj["href"];
				if (jtoken != null)
					mExtrudedDirection = jtoken.Value<int>();
				else
					ExtrudedDirection = mDatabase.parseJObject<IfcDirection>(jobj);
			}
			JToken token = obj.GetValue("Depth", StringComparison.InvariantCultureIgnoreCase);
			if(token != null)
				mDepth= token.Value<double>();
		}
		protected override void setJSON(JObject obj, BaseClassIfc host,  HashSet<int> processed)
		{
			base.setJSON(obj, host, processed);

			obj["ExtrudedDirection"] = ExtrudedDirection.getJson(this, processed);
			obj["Depth"] = mDepth;
		}
	}
}
