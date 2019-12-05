using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#region Additional Namespaces
using NorthwindSystem.BLL;
using NorthwindSystem.Data;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core;

#endregion
namespace WebApp.NorthwindPages
{
    public partial class ProductCRUD : System.Web.UI.Page
    {
        //this collection is used by the specialized error handling code
        List<string> errormsgs = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            //this following code replaces the basic Label MessageLabel control
            //   to clear old message
            //the control being used for the error handling display of messages
            //   will be a DataList
            //the clearing of the DataList control is done by assinging a null
            //   as the collection and binding that emplty collection to the
            //   control
            Message.DataSource = null;
            Message.DataBind();

            if (!Page.IsPostBack)
            {
                BindProductList();
                BindCategoryList();
                BindSupplierList();
            }

        }

        //use this method to discover the inner most error message.
        //this rotuing has been created by the user
        protected Exception GetInnerException(Exception ex)
        {
            //drill down to the inner most exception
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        //use this method to load a DataList with a variable
        //number of message lines.
        //each line is a string
        //the strings (lines) are passed to this routine in
        //   a List<string>
        //second parameter is the bootstrap cssclass
        protected void LoadMessageDisplay(List<string> errormsglist, string cssclass)
        {
            Message.CssClass = cssclass;
            Message.DataSource = errormsglist;
            Message.DataBind();
        }
        #region Loading and Binding of DDLs
        protected void BindCategoryList()
        {
            //standard lookup
            try
            {
                CategoryController sysmgr = new CategoryController();
                List<Category> info = null;
                info = sysmgr.Categories_List();
                info.Sort((x, y) => x.CategoryName.CompareTo(y.CategoryName));
                CategoryList.DataSource = info;
                CategoryList.DataTextField = nameof(Category.CategoryName);
                CategoryList.DataValueField = nameof(Category.CategoryID);
                CategoryList.DataBind();
                CategoryList.Items.Insert(0, "select...");

            }
            catch (Exception ex)
            {
                errormsgs.Add(GetInnerException(ex).ToString());
                LoadMessageDisplay(errormsgs, "alert alert-danger");
            }
        }
        protected void BindSupplierList()
        {
            //standard lookup
            try
            {
                SupplierController sysmgr = new SupplierController();
                List<Supplier> info = null;
                info = sysmgr.Supplier_List();
                info.Sort((x, y) => x.ContactName.CompareTo(y.ContactName));
                SupplierList.DataSource = info;
                SupplierList.DataTextField = nameof(Supplier.ContactName);
                SupplierList.DataValueField = nameof(Supplier.SupplierID);
                SupplierList.DataBind();
                SupplierList.Items.Insert(0, "select...");

            }
            catch (Exception ex)
            {
                errormsgs.Add(GetInnerException(ex).ToString());
                LoadMessageDisplay(errormsgs, "alert alert-danger");
            }
        }
        protected void BindProductList()
        {
            //standard lookup
            try
            {
                ProductController sysmgr = new ProductController();
                List<Product> info = null;
                info = sysmgr.Products_List();
                info.Sort((x, y) => x.ProductName.CompareTo(y.ProductName));
                ProductList.DataSource = info;
                ProductList.DataTextField = nameof(Product.ProductName);
                ProductList.DataValueField = nameof(Product.ProductID);
                ProductList.DataBind();
                ProductList.Items.Insert(0, "select...");

            }
            catch (Exception ex)
            {
                errormsgs.Add(GetInnerException(ex).ToString());
                LoadMessageDisplay(errormsgs, "alert alert-danger");
            }
        }

        #endregion

        protected void Clear_Click(object sender, EventArgs e)
        {
            ProductID.Text = "";
            ProductName.Text = "";
            QuantityPerUnit.Text = "";
            UnitPrice.Text = "";
            UnitsInStock.Text = "";
            UnitsOnOrder.Text = "";
            ReorderLevel.Text = "";
            Discontinued.Checked = false;
            ProductList.ClearSelection();
            CategoryList.ClearSelection();
            SupplierList.ClearSelection();
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            if (ProductList.SelectedIndex == 0)
            {
                errormsgs.Add("Select a product to maintain");
                LoadMessageDisplay(errormsgs, "alert alert-info");
            }
            else
            {
                //standard lookup
                try
                {
                    ProductController sysmgr = new ProductController();
                    Product info = null;
                    info = sysmgr.Products_FindByID(int.Parse(ProductList.SelectedValue));
                    if (info == null)
                    {
                        errormsgs.Add("Product is no longer on file.");
                        LoadMessageDisplay(errormsgs, "alert alert-info");
                        //optionally
                        //refresh the page
                        //    clear out the fields
                        //    reload the ProductList to remove of product that was not found
                        Clear_Click(sender, e);
                        BindProductList();
                    }
                    else
                    {
                        ProductID.Text = info.ProductID.ToString();
                        ProductName.Text = info.ProductName;
                        QuantityPerUnit.Text =
                            info.QuantityPerUnit == null ? "" : info.QuantityPerUnit;
                        UnitPrice.Text =
                            info.UnitPrice.HasValue ? string.Format("{0:0.00}", info.UnitPrice.Value) : "";
                        UnitsInStock.Text =
                            info.UnitsInStock.HasValue ? info.UnitsInStock.Value.ToString() : "";
                        UnitsOnOrder.Text =
                            info.UnitsOnOrder.HasValue ? info.UnitsOnOrder.Value.ToString() : "";
                        ReorderLevel.Text =
                            info.ReorderLevel.HasValue ? info.ReorderLevel.Value.ToString() : "";
                        Discontinued.Checked = info.Discontinued;
                        if (info.CategoryID.HasValue)
                        {
                            CategoryList.SelectedValue = info.CategoryID.ToString();
                        }
                        else
                        {
                            CategoryList.SelectedIndex = 0;
                        }
                        if (info.SupplierID.HasValue)
                        {
                            SupplierList.SelectedValue = info.SupplierID.ToString();
                        }
                        else
                        {
                            SupplierList.SelectedIndex = 0;
                        }
                    }


                }
                catch (Exception ex)
                {
                    errormsgs.Add(GetInnerException(ex).ToString());
                    LoadMessageDisplay(errormsgs, "alert alert-danger");
                }
            }
        }

        protected void AddProduct_Click(object sender, EventArgs e)
        {
            //ensure validation is still good
            if (Page.IsValid)
            {
                //event code validation that was not accomplished on
                //   the web form
                //examples
                //Assume that the CategoryID is required
                if (CategoryList.SelectedIndex == 0)
                {
                    errormsgs.Add("Category is required");
                }
                if (QuantityPerUnit.Text.Length > 20)
                {
                    errormsgs.Add("Quantity per Unit is limited to 20 characters");
                }

                //check if click event validation is good
                if (errormsgs.Count > 0)
                {
                    LoadMessageDisplay(errormsgs, "alert alert-info");
                }
                else
                {
                    //assume taht the data is validate to our knowledge
                    try
                    {
                        //standard add to a database
                        //connect to the appropriate controller
                        ProductController sysmgr = new ProductController();
                        //create and load an instance of the entity record
                        //  since there was no constructor placed in the
                        //  entity, when one creaes the instance the
                        //  default system construtor will be used
                        Product item = new Product();
                        //what about ProductID??
                        //   since ProductID is an identity field it does NOT
                        //   need to be loaded into the new instance
                        item.ProductName = ProductName.Text.Trim();
                        if (CategoryList.SelectedIndex == 0)
                        {
                            item.CategoryID = null;
                        }
                        else
                        {
                            item.CategoryID = int.Parse(CategoryList.SelectedValue);
                        }
                        if (SupplierList.SelectedIndex == 0)
                        {
                            item.SupplierID = null;
                        }
                        else
                        {
                            item.SupplierID = int.Parse(SupplierList.SelectedValue);
                        }
                        item.QuantityPerUnit =
                            string.IsNullOrEmpty(QuantityPerUnit.Text) ? null : QuantityPerUnit.Text;
                        if(string.IsNullOrEmpty(UnitPrice.Text))
                        {
                            item.UnitPrice = null;
                        }
                        else
                        {
                            item.UnitPrice = decimal.Parse(UnitPrice.Text);
                        }
                        if (string.IsNullOrEmpty(UnitsInStock.Text))
                        {
                            item.UnitsInStock = null;
                        }
                        else
                        {
                            item.UnitsInStock = Int16.Parse(UnitsInStock.Text);
                        }
                        if (string.IsNullOrEmpty(UnitsOnOrder.Text))
                        {
                            item.UnitsOnOrder = null;
                        }
                        else
                        {
                            item.UnitsOnOrder = Int16.Parse(UnitsOnOrder.Text);
                        }
                        if (string.IsNullOrEmpty(ReorderLevel.Text))
                        {
                            item.ReorderLevel = null;
                        }
                        else
                        {
                            item.ReorderLevel = Int16.Parse(ReorderLevel.Text);
                        }
                        //what about Discontinued??
                        item.Discontinued = false;
                        //issue the BLL call
                        int newProductID = sysmgr.Products_Add(item);
                        //give feedback
                        //if you get to execute the feedback code, it means
                        //    that the product has been successfully added to
                        //    the database
                        ProductID.Text = newProductID.ToString();
                        errormsgs.Add("Product has been added");
                        LoadMessageDisplay(errormsgs, "alert alert-success");
                        //is there any other controls on the form that need to be refreshed
                        BindProductList(); //by default, list will be at index 0
                        ProductList.SelectedValue = ProductID.Text;
                    }
                    catch (DbUpdateException ex)
                    {
                        UpdateException updateException = (UpdateException)ex.InnerException;
                        if (updateException.InnerException != null)
                        {
                            errormsgs.Add(updateException.InnerException.Message.ToString());
                        }
                        else
                        {
                            errormsgs.Add(updateException.Message);
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                errormsgs.Add(validationError.ErrorMessage);
                            }
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (Exception ex)
                    {
                        errormsgs.Add(GetInnerException(ex).ToString());
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                }
            }
        }

        protected void UpdateProduct_Click(object sender, EventArgs e)
        {
            //ensure validation is still good
            if (Page.IsValid)
            {
                //event code validation that was not accomplished on
                //   the web form
                //examples
                //Assume that the CategoryID is required
                if (CategoryList.SelectedIndex == 0)
                {
                    errormsgs.Add("Category is required");
                }
                if (QuantityPerUnit.Text.Length > 20)
                {
                    errormsgs.Add("Quantity per Unit is limited to 20 characters");
                }

                //on updae, ensure you have your primary key value
                int productid = 0;
                if (string.IsNullOrEmpty(ProductID.Text))
                {
                    errormsgs.Add("Search for a product to update");
                }
                else if (!int.TryParse(ProductID.Text, out productid))
                {
                    errormsgs.Add("Product id is invalid");
                }
                else if (productid < 1)
                {
                    errormsgs.Add("Product id is invalid");
                }

                //check if click event validation is good
                if (errormsgs.Count > 0)
                {
                    LoadMessageDisplay(errormsgs, "alert alert-info");
                }
                else
                {
                    //assume taht the data is validate to our knowledge
                    try
                    {
                        //standard update to a database
                        //connect to the appropriate controller
                        ProductController sysmgr = new ProductController();
                        //create and load an instance of the entity record
                        //  since there was no constructor placed in the
                        //  entity, when one creaes the instance the
                        //  default system construtor will be used
                        Product item = new Product();
                        //ensure yo include the primary key
                        item.ProductID = productid;
                        item.ProductName = ProductName.Text.Trim();
                        if (CategoryList.SelectedIndex == 0)
                        {
                            item.CategoryID = null;
                        }
                        else
                        {
                            item.CategoryID = int.Parse(CategoryList.SelectedValue);
                        }
                        if (SupplierList.SelectedIndex == 0)
                        {
                            item.SupplierID = null;
                        }
                        else
                        {
                            item.SupplierID = int.Parse(SupplierList.SelectedValue);
                        }
                        item.QuantityPerUnit =
                            string.IsNullOrEmpty(QuantityPerUnit.Text) ? null : QuantityPerUnit.Text;
                        if (string.IsNullOrEmpty(UnitPrice.Text))
                        {
                            item.UnitPrice = null;
                        }
                        else
                        {
                            item.UnitPrice = decimal.Parse(UnitPrice.Text);
                        }
                        if (string.IsNullOrEmpty(UnitsInStock.Text))
                        {
                            item.UnitsInStock = null;
                        }
                        else
                        {
                            item.UnitsInStock = Int16.Parse(UnitsInStock.Text);
                        }
                        if (string.IsNullOrEmpty(UnitsOnOrder.Text))
                        {
                            item.UnitsOnOrder = null;
                        }
                        else
                        {
                            item.UnitsOnOrder = Int16.Parse(UnitsOnOrder.Text);
                        }
                        if (string.IsNullOrEmpty(ReorderLevel.Text))
                        {
                            item.ReorderLevel = null;
                        }
                        else
                        {
                            item.ReorderLevel = Int16.Parse(ReorderLevel.Text);
                        }
                        //actually current valud of discontinued is needed
                        item.Discontinued = Discontinued.Checked;
                        //issue the BLL call
                        int rowsaffected = sysmgr.Products_Update(item);
                        //give feedback
                        if (rowsaffected > 0)
                        {
                           
                            errormsgs.Add("Product has been updated");
                            LoadMessageDisplay(errormsgs, "alert alert-success");
                            //is there any other controls on the form that need to be refreshed
                            BindProductList(); //by default, list will be at index 0
                            ProductList.SelectedValue = ProductID.Text;
                        }
                        else
                        {
                            errormsgs.Add("Product has not been updated. Product was not found");
                            LoadMessageDisplay(errormsgs, "alert alert-info");
                            //is there any other controls on the form that need to be refreshed
                            BindProductList(); //by default, list will be at index 0
                            
                            //optionally you could clear your fields
                        }
                       
                    }
                    catch (DbUpdateException ex)
                    {
                        UpdateException updateException = (UpdateException)ex.InnerException;
                        if (updateException.InnerException != null)
                        {
                            errormsgs.Add(updateException.InnerException.Message.ToString());
                        }
                        else
                        {
                            errormsgs.Add(updateException.Message);
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                errormsgs.Add(validationError.ErrorMessage);
                            }
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (Exception ex)
                    {
                        errormsgs.Add(GetInnerException(ex).ToString());
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                }
            }
        }

        protected void RemoveProduct_Click(object sender, EventArgs e)
        {
           
                //event code validation that was not accomplished on
                //   the web form
                //examples
                //Assume that the CategoryID is required
               

                //on delete, ensure you have your primary key value
                int productid = 0;
                if (string.IsNullOrEmpty(ProductID.Text))
                {
                    errormsgs.Add("Search for a product to update");
                }
                else if (!int.TryParse(ProductID.Text, out productid))
                {
                    errormsgs.Add("Product id is invalid");
                }
                else if (productid < 1)
                {
                    errormsgs.Add("Product id is invalid");
                }

                //check if click event validation is good
                if (errormsgs.Count > 0)
                {
                    LoadMessageDisplay(errormsgs, "alert alert-info");
                }
                else
                {
                    //assume taht the data is validate to our knowledge
                    try
                    {
                        //standard delete to a database
                        //connect to the appropriate controller
                        ProductController sysmgr = new ProductController();
                      
                        //issue the BLL call
                        int rowsaffected = sysmgr.Products_Delete(productid);
                        //give feedback
                        if (rowsaffected > 0)
                        {

                            errormsgs.Add("Product has been discontinued");
                            LoadMessageDisplay(errormsgs, "alert alert-success");
                            //is there any other controls on the form that need to be refreshed
                            BindProductList(); //by default, list will be at index 0
                            ProductList.SelectedValue = ProductID.Text;  //logical delete
                            Discontinued.Checked = true; //logical delete
                        }
                        else
                        {
                            errormsgs.Add("Product has not been discontinued. Product was not found");
                            LoadMessageDisplay(errormsgs, "alert alert-warning");
                            //is there any other controls on the form that need to be refreshed
                            BindProductList(); //by default, list will be at index 0

                            //optionally you could clear your fields
                        }

                    }
                    catch (DbUpdateException ex)
                    {
                        UpdateException updateException = (UpdateException)ex.InnerException;
                        if (updateException.InnerException != null)
                        {
                            errormsgs.Add(updateException.InnerException.Message.ToString());
                        }
                        else
                        {
                            errormsgs.Add(updateException.Message);
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                errormsgs.Add(validationError.ErrorMessage);
                            }
                        }
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                    catch (Exception ex)
                    {
                        errormsgs.Add(GetInnerException(ex).ToString());
                        LoadMessageDisplay(errormsgs, "alert alert-danger");
                    }
                }

        }
    }
}