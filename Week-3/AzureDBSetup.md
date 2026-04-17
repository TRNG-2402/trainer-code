# Provisioning and Connecting to an Azure SQL Database (Basic Tier)

## 1. Provisioning the Basic Tier Database

1. Log in to the [Azure Portal](https://portal.azure.com) and search for **SQL databases**.
2. Click **+ Create**.
3. **Basics Tab configuration:**
   * **Subscription & Resource group:** Select your target subscription and assign or create a resource group.
   * **Database name:** Input a unique identifier.
   * **Server:** Click **Create new**. Enter a globally unique server name, select the deployment region, choose **Use SQL authentication**, and define the Server admin login and password.
   * **Want to use SQL elastic pool:** Select **No**.
   * **Workload environment:** Select **Development**.
4. **Compute + Storage configuration:**
   * Click **Configure database**.
   * Bypass the vCore defaults by selecting the **Looking for basic, standard, premium?** link.
   * Select the **Basic** service tier. Verify the limits display **5 DTUs** and **2 GB** max size.
   * Click **Apply**.
5. **Networking Tab configuration:**
   * **Connectivity method:** Select **Public endpoint**.
   * **Allow Azure services and resources to access this server:** Set to **No**.
   * **Add current client IP address:** Set to **Yes** (mandatory for local firewall access).
6. Click **Review + create**, verify the Basic tier pricing estimate, and click **Create**.

## 2. Retrieving Connection Details

1. Upon successful deployment, click **Go to resource**.
2. On the Overview pane, locate the **Server name**.
3. Copy the exact URI (format: `<your-server-name>.database.windows.net`).

## 3. Connecting via Visual Studio Code

1. Open VS Code and navigate to the **Extensions** view (`Ctrl+Shift+X` / `Cmd+Shift+X`).
2. Install the **SQL Server (mssql)** extension published by Microsoft.
3. Open the **SQL Server** view via the Activity Bar.
4. In the Connections pane, click the **+ (Add Connection)** icon.
5. Input the parameters as prompted:
   * **Server name:** Paste the copied server URI.
   * **Database name:** Enter the specific database name defined in Step 1.
   * **Authentication Type:** Select **SQL Login**.
   * **User name:** Enter the Server admin login.
   * **Password:** Enter the admin password.
   * **Profile Name:** Assign a local alias (e.g., `AzureSQL-Basic`).
6. Press **Enter** to establish the connection. Once connected, right-click the database node to open a **New Query** window.