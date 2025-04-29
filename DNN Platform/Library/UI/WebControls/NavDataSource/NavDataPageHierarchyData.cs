// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls;

using System.Web.UI;

using DotNetNuke.Entities.Portals;

public class NavDataPageHierarchyData : IHierarchyData, INavigateUIData
{
    private readonly DNNNode objNode;

    /// <summary>Initializes a new instance of the <see cref="NavDataPageHierarchyData"/> class.</summary>
    /// <param name="obj">The node.</param>
    public NavDataPageHierarchyData(DNNNode obj)
    {
        this.objNode = obj;
    }

    /// <summary>Gets nodes image.</summary>
    /// <value>Returns nodes image.</value>
    public virtual string ImageUrl
    {
        get
        {
            if (string.IsNullOrEmpty(this.objNode.Image) || this.objNode.Image.StartsWith("/"))
            {
                return this.objNode.Image;
            }
            else
            {
                return PortalController.Instance.GetCurrentPortalSettings().HomeDirectory + this.objNode.Image;
            }
        }
    }

    /// <summary>Gets a value indicating whether indicates whether the hierarchical data node that the IHierarchyData object represents has any child nodes.</summary>
    /// <value>Indicates whether the hierarchical data node that the IHierarchyData object represents has any child nodes.</value>
    public virtual bool HasChildren
    {
        get
        {
            return this.objNode.HasNodes;
        }
    }

    /// <summary>Gets the hierarchical path of the node.</summary>
    /// <value>The hierarchical path of the node.</value>
    public virtual string Path
    {
        get
        {
            return this.GetValuePath(this.objNode);
        }
    }

    /// <summary>Gets the hierarchical data node that the IHierarchyData object represents.</summary>
    /// <value>The hierarchical data node that the IHierarchyData object represents.</value>
    public virtual object Item
    {
        get
        {
            return this.objNode;
        }
    }

    /// <summary>Gets the name of the type of Object contained in the Item property.</summary>
    /// <value>The name of the type of Object contained in the Item property.</value>
    public virtual string Type
    {
        get
        {
            return "NavDataPageHierarchyData";
        }
    }

    /// <summary>Gets node name.</summary>
    /// <value>Returns node name.</value>
    public virtual string Name
    {
        get
        {
            return this.GetSafeValue(this.objNode.Text, string.Empty);
        }
    }

    /// <summary>Gets value path of node.</summary>
    /// <value>Returns value path of node.</value>
    public virtual string Value
    {
        get
        {
            return this.GetValuePath(this.objNode);
        }
    }

    /// <summary>Gets node navigation url.</summary>
    /// <value>Returns node navigation url.</value>
    public virtual string NavigateUrl
    {
        get
        {
            return this.GetSafeValue(this.objNode.NavigateURL, string.Empty);
        }
    }

    /// <summary>Gets node description.</summary>
    /// <value>Returns Node description.</value>
    public virtual string Description
    {
        get
        {
            return this.GetSafeValue(this.objNode.ToolTip, string.Empty);
        }
    }

    /// <summary>Gets an enumeration object that represents all the child nodes of the current hierarchical node.</summary>
    /// <returns>A collection of nodes.</returns>
    public virtual IHierarchicalEnumerable GetChildren()
    {
        var objNodes = new NavDataPageHierarchicalEnumerable();
        if (this.objNode != null)
        {
            foreach (DNNNode objNode in this.objNode.DNNNodes)
            {
                objNodes.Add(new NavDataPageHierarchyData(objNode));
            }
        }

        return objNodes;
    }

    /// <summary>Gets an enumeration object that represents the parent node of the current hierarchical node.</summary>
    /// <returns>The parent node or <see langword="null"/>.</returns>
    public virtual IHierarchyData GetParent()
    {
        if (this.objNode != null)
        {
            return new NavDataPageHierarchyData(this.objNode.ParentNode);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.objNode.Text;
    }

    /// <summary>Helper function to handle cases where property is <see langword="null"/>.</summary>
    /// <param name="value">Value to evaluate for <see langword="null"/>.</param>
    /// <param name="def">If <see langword="null"/>, return this default.</param>
    /// <returns><paramref name="value"/> or <paramref name="def"/> if <paramref name="value"/> is <see langword="null"/>.</returns>
    private string GetSafeValue(string value, string def)
    {
        if (value != null)
        {
            return value;
        }
        else
        {
            return def;
        }
    }

    /// <summary>Computes valuepath necessary for ASP.NET controls to guarantee uniqueness.</summary>
    /// <param name="objNode"></param>
    /// <returns>ValuePath.</returns>
    /// <remarks>Not sure if it is ok to hardcode the "\" separator, but also not sure where I would get it from.</remarks>
    private string GetValuePath(DNNNode objNode)
    {
        DNNNode objParent = objNode.ParentNode;
        string strPath = this.GetSafeValue(objNode.Key, string.Empty);
        do
        {
            if (objParent == null || objParent.Level == -1)
            {
                break;
            }

            strPath = this.GetSafeValue(objParent.Key, string.Empty) + "\\" + strPath;
            objParent = objParent.ParentNode;
        }
        while (true);
        return strPath;
    }
}
